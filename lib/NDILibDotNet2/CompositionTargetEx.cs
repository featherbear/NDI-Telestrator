using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NewTek.NDI
{
    public static class CompositionTargetEx
    {
        private static TimeSpan _last = TimeSpan.Zero;
        private static event EventHandler<RenderingEventArgs> _FrameUpdating;
        public static event EventHandler<RenderingEventArgs> FrameUpdating
        {
            add
            {
                if (_FrameUpdating == null)
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                _FrameUpdating += value;
            }
            remove
            {
                _FrameUpdating -= value;
                if (_FrameUpdating == null) CompositionTarget.Rendering -= CompositionTarget_Rendering;
            }
        }
        static void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;
            if (args.RenderingTime == _last) return;
            _last = args.RenderingTime;
            _FrameUpdating(sender, args);
        }
    }
}
