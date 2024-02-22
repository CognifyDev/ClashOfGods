using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Listener.Event.Impl.ICutscene
{
    public class IntroCutsceneDestroyEvent : IntroCutsceneEvent
    {
        public IntroCutsceneDestroyEvent(IntroCutscene intro) : base(intro) { }
    }
}
