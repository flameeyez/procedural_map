using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace procedural_map {
    enum TIMED_STRING_STATE {
        FADING_IN,
        DISPLAYING,
        FADING_OUT,
        DEAD
    }

    class TimedString {
        private double _age;
        private int _lifespan;
        private TIMED_STRING_STATE _state;
        private byte _opacity;
        private static byte _opacitystep = 15;

        public bool Dead { get { return _state == TIMED_STRING_STATE.DEAD; } }
        public string String { get; set; }
        public TimedString(string str, int milliseconds = 5000) {
            String = str;
            _age = 0;
            _opacity = 0;
            _lifespan = milliseconds;
            _state = TIMED_STRING_STATE.FADING_IN;
        }

        public void Draw(CanvasAnimatedDrawEventArgs args, Vector2 position) {
            if (Dead) { return; }
            args.DrawingSession.DrawText(String, position, Color.FromArgb(_opacity, 255, 255, 255));
        }

        public void Update(CanvasAnimatedUpdateEventArgs args) {
            switch (_state) {
                case TIMED_STRING_STATE.FADING_IN:
                    _opacity += _opacitystep;
                    if (_opacity == 255) {
                        _state = TIMED_STRING_STATE.DISPLAYING;
                    }
                    break;
                case TIMED_STRING_STATE.DISPLAYING:
                    _age += args.Timing.ElapsedTime.TotalMilliseconds;
                    if (_age >= _lifespan) {
                        _state = TIMED_STRING_STATE.FADING_OUT;
                    }
                    break;
                case TIMED_STRING_STATE.FADING_OUT:
                    _opacity -= 17;
                    if (_opacity == 0) {
                        _state = TIMED_STRING_STATE.DEAD;
                    }
                    break;
            }
        }
    }
}
