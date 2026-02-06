using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace COG.MetaContext;

public class SpriteLoader : Image
{
    private readonly float pixelsPerUnit;
    private readonly ITextureLoader textureLoader;
    private Sprite sprite = null!;

    public SpriteLoader(ITextureLoader textureLoader, float pixelsPerUnit)
    {
        this.textureLoader = textureLoader;
        this.pixelsPerUnit = pixelsPerUnit;
    }

    public Sprite GetSprite()
    {
        if (!sprite) sprite = textureLoader.GetTexture().ToSprite(pixelsPerUnit);
        sprite.hideFlags = textureLoader.GetTexture().hideFlags;
        return sprite;
    }

    public static SpriteLoader FromResource(string address, float pixelsPerUnit)
    {
        return new SpriteLoader(new ResourceTextureLoader(address), pixelsPerUnit);
    }
}

public interface ITextureLoader
{
    Texture2D GetTexture();
}

public interface IDividedSpriteLoader
{
    int Length { get; }
    Sprite GetSprite(int index);

    Image AsLoader(int index)
    {
        return new WrapSpriteLoader(() => GetSprite(index));
    }
}

public static class GraphicsHelper
{
    internal static d_LoadImage iCall_LoadImage = null!;

    public static Sprite ToSprite(this Texture2D texture, float pixelsPerUnit)
    {
        return ToSprite(texture, new Rect(0, 0, texture.width, texture.height), pixelsPerUnit);
    }

    public static Sprite ToSprite(this Texture2D texture, Rect rect, float pixelsPerUnit)
    {
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    public static Sprite ToSprite(this Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit)
    {
        return Sprite.Create(texture, rect, pivot, pixelsPerUnit);
    }

    public static Texture2D LoadTextureFromDisk(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                var byteTexture = File.ReadAllBytes(path);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
        }
        catch
        {
            Main.Logger.LogError("Error loading texture from disk: " + path);
        }

        return null!;
    }

    public static Texture2D LoadTextureFromResources(string path)
    {
        Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null!;
        var byteTexture = new byte[stream.Length];
        stream.Read(byteTexture, 0, (int)stream.Length);
        LoadImage(texture, byteTexture, false);
        return texture;
    }

    public static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
    {
        if (iCall_LoadImage == null)
            iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
        var il2cppArray = (Il2CppStructArray<byte>)data;
        return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
    }

    internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
}

public class ResourceTextureLoader : ITextureLoader
{
    private readonly string address;
    private Texture2D texture;

    public ResourceTextureLoader(string address)
    {
        this.address = address;
    }

    public Texture2D GetTexture()
    {
        if (!texture) texture = GraphicsHelper.LoadTextureFromResources(address);
        return texture!;
    }
}

public class DiskTextureLoader : ITextureLoader
{
    private readonly string address;
    private bool isUnloadAsset;
    private Texture2D texture = null!;

    public DiskTextureLoader(string address)
    {
        this.address = address;
    }

    public Texture2D GetTexture()
    {
        if (!texture)
        {
            texture = GraphicsHelper.LoadTextureFromDisk(address);
            if (isUnloadAsset) texture.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
        }

        return texture;
    }

    public DiskTextureLoader MarkAsUnloadAsset()
    {
        isUnloadAsset = true;
        return this;
    }
}

public class XOnlyDividedSpriteLoader : Image, IDividedSpriteLoader
{
    private readonly float pixelsPerUnit;
    private readonly ITextureLoader texture;
    private int? division, size;
    public Vector2 Pivot = new(0.5f, 0.5f);
    private Sprite[] sprites;

    public XOnlyDividedSpriteLoader(ITextureLoader textureLoader, float pixelsPerUnit, int x, bool isSize = false)
    {
        this.pixelsPerUnit = pixelsPerUnit;
        if (isSize)
        {
            size = x;
            division = null;
        }
        else
        {
            division = x;
            size = null;
        }

        sprites = null!;
        texture = textureLoader;
    }

    public Sprite GetSprite(int index)
    {
        if (!size.HasValue || !division.HasValue || sprites == null)
        {
            var texture2D = texture.GetTexture();
            if (size == null)
                size = texture2D.width / division;
            else if (division == null)
                division = texture2D.width / size!;
            sprites = new Sprite[division!.Value];
        }

        if (!sprites[index])
        {
            var texture2D = texture.GetTexture();
            sprites[index] = texture2D.ToSprite(new Rect(index * size!.Value, 0, size!.Value, texture2D.height), Pivot,
                pixelsPerUnit);
        }

        return sprites[index];
    }

    public int Length
    {
        get
        {
            if (!division.HasValue) GetSprite(0);
            return division!.Value;
        }
    }

    public Sprite GetSprite()
    {
        return GetSprite(0);
    }
    public static XOnlyDividedSpriteLoader FromResource(string address, float pixelsPerUnit, int x, bool isSize = false)
    {
        return new XOnlyDividedSpriteLoader(new ResourceTextureLoader(address), pixelsPerUnit, x, isSize);
    }
}

public class DividedSpriteLoader : Image, IDividedSpriteLoader
{
    private readonly float pixelsPerUnit;
    private readonly ITextureLoader texture;
    private Tuple<int, int> division, size;
    public Vector2 Pivot = new(0.5f, 0.5f);
    private Sprite[] sprites;

    public DividedSpriteLoader(ITextureLoader textureLoader, float pixelsPerUnit, int x, int y, bool isSize = false)
    {
        this.pixelsPerUnit = pixelsPerUnit;
        if (isSize)
        {
            size = new Tuple<int, int>(x, y);
            division = null;
        }
        else
        {
            division = new Tuple<int, int>(x, y);
            size = null;
        }

        sprites = null!;
        texture = textureLoader;
    }

    public Sprite GetSprite(int index)
    {
        if (size == null || division == null || sprites == null)
        {
            var texture2D = texture.GetTexture();
            if (size == null)
                size = new Tuple<int, int>(texture2D.width / division!.Item1, texture2D.height / division!.Item2);
            else if (division == null)
                division = new Tuple<int, int>(texture2D.width / size!.Item1, texture2D.height / size!.Item2);
            sprites = new Sprite[division!.Item1 * division!.Item2];
        }

        if (!sprites[index])
        {
            var texture2D = texture.GetTexture();
            var _x = index % division!.Item1;
            var _y = index / division!.Item1;
            sprites[index] =
                texture2D.ToSprite(
                    new Rect(_x * size.Item1, (division.Item2 - _y - 1) * size.Item2, size.Item1, size.Item2), Pivot,
                    pixelsPerUnit);
        }

        return sprites[index];
    }

    public Image AsLoader(int index)
    {
        return new WrapSpriteLoader(() => GetSprite(index));
    }

    public int Length
    {
        get
        {
            if (division == null) GetSprite(0);
            return division!.Item1 * division!.Item2;
        }
    }

    public Sprite GetSprite()
    {
        return GetSprite(0);
    }

    public static DividedSpriteLoader FromResource(string address, float pixelsPerUnit, int x, int y,
        bool isSize = false)
    {
        return new DividedSpriteLoader(new ResourceTextureLoader(address), pixelsPerUnit, x, y, isSize);
    }
}
public interface Image
{
    internal Sprite GetSprite();
}
public interface ISpriteLoader : Image
{
}
public class WrapSpriteLoader : ISpriteLoader
{
    Func<Sprite> supplier;

    public WrapSpriteLoader(Func<Sprite> supplier)
    {
        this.supplier = supplier;
    }

    public Sprite GetSprite() => supplier.Invoke();
}