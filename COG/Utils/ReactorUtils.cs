using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace COG.Utils
{
    public static class ReactorUtils
    {
        public static byte[] ReadFully(this System.IO.Stream input)
        {
            using System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            input.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        public static T DontUnload<T>(this T obj) where T : UnityEngine.Object
        {
            obj.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return obj;
        }
    }
}
