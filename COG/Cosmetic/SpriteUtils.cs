using System;
using System.Buffers;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace COG.Cosmetics;

public static class SpriteUtils
{
    private static Sprite? _emptySprite;
    public static Sprite EmptySprite
    {
        get
        {
            if (_emptySprite != null) return _emptySprite;

            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.clear);
            tex.Apply();

            _emptySprite = Sprite.Create(
                tex,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                100f
            );
            return _emptySprite;
        }
    }

    public static Sprite? LoadSpriteFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var sprite = LoadSpriteFromStream(fs);
            if (sprite != null)
                sprite.name = Path.GetFileName(filePath);
            return sprite;
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"[Cosmetics] Error loading sprite from '{filePath}':\n{e}");
            return null;
        }
    }

    public static Sprite? LoadSpriteFromStream(Stream stream)
        => LoadSpriteFromStream(stream, 0, (uint)stream.Length);

    public static Sprite? LoadSpriteFromStream(Stream stream, long start, uint length)
    {
        if (length == 0) return null;
        try
        {
            var texture = TextureFromStream(stream, start, length);
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"[Cosmetics] Error loading sprite from stream:\n{e}");
            return null;
        }
    }

    private static Texture2D TextureFromStream(Stream stream, long start, uint length)
    {
        stream.Seek(start, SeekOrigin.Begin);

        var il2CppBytes = new Il2CppStructArray<byte>(length);
        CopyFromStream(il2CppBytes, stream, (int)length);

        var texture = new Texture2D(2, 2);
        texture.LoadImage(il2CppBytes);
        return texture;
    }

    private static unsafe void CopyFromStream(Il2CppStructArray<byte> destination, Stream stream, int length)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            var read = stream.Read(buffer, 0, length);
            if (read != length)
                throw new EndOfStreamException(
                    $"[Cosmetics] Expected {length} bytes but only read {read}.");

            fixed (byte* src = buffer)
            {
                var dst = (byte*)System.IntPtr.Add(destination.Pointer, 4 * System.IntPtr.Size).ToPointer();
                Buffer.MemoryCopy(src, dst, length, length);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
