using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace procedural_map {
    class SpriteAnimationSet {
        public CanvasBitmap CharacterMap;

        private Dictionary<SPRITE_ANIMATION, SpriteAnimation> Animations = new Dictionary<SPRITE_ANIMATION, SpriteAnimation>();
        public SpriteAnimation CurrentAnimation { get; set; }
        private SPRITE_ANIMATION _currentAnimationState;
        public SPRITE_ANIMATION CurrentAnimationState {
            get { return _currentAnimationState; }
            internal set {
                _currentAnimationState = value;
                CurrentAnimation = Animations[_currentAnimationState];
            }
        }
        public int Resolution { get; set; }

        public SpriteAnimationSet(CanvasBitmap characterMap, int resolution) {
            CharacterMap = characterMap;
            Resolution = resolution;
        }

        internal void SetAnimation(SPRITE_ANIMATION newAnimationState) {
            if (newAnimationState != CurrentAnimationState) {
                CurrentAnimationState = newAnimationState;
                CurrentAnimation.Reset();
            }
        }

        //public void Draw(Point position, CanvasAnimatedDrawEventArgs args)
        //{
        //    args.DrawingSession.DrawImage(CharacterMap, (float)position.X, (float)position.Y, new Rect(CurrentAnimation.CurrentFrame.X * Resolution, CurrentAnimation.CurrentFrame.Y * Resolution, Resolution, Resolution));
        //}

        public void Update(CanvasAnimatedUpdateEventArgs args) {
            CurrentAnimation.Update(args);
        }

        //internal void Add(SPRITE_ANIMATION animation)
        //{
        //    Animations.Add(animation, SpriteAnimationDefinitions.Copy(animation));
        //}
    }
}
