using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace procedural_map {
    enum SPRITE_ANIMATION {
        SPELLCAST_UP,
        SPELLCAST_LEFT,
        SPELLCAST_DOWN,
        SPELLCAST_RIGHT,
        ATTACK_UP,
        ATTACK_LEFT,
        ATTACK_DOWN,
        ATTACK_RIGHT,
        WALK_UP,
        WALK_LEFT,
        WALK_DOWN,
        WALK_RIGHT,
        IDLE_UP,
        IDLE_LEFT,
        IDLE_DOWN,
        IDLE_RIGHT,
        SLASH_UP,
        SLASH_LEFT,
        SLASH_DOWN,
        SLASH_RIGHT,
        SHOOT_UP,
        SHOOT_LEFT,
        SHOOT_DOWN,
        SHOOT_RIGHT,
        KO
    }

    class SpriteAnimation {
        private bool _loops;
        public List<SpriteAnimationFrame> Frames = new List<SpriteAnimationFrame>();
        private int _currentFrameIndex;
        private SpriteAnimationFrame _currentFrame { get { return Frames[_currentFrameIndex]; } }
        private double _frameDurationInMilliseconds;
        private double _millisecondsSinceLastUpdate;

        internal void Draw(SpriteSheet spriteSheet, PointInt position, CanvasAnimatedDrawEventArgs args) {
            args.DrawingSession.DrawImage(spriteSheet.Image,
                destinationRectangle: new Rect(position.X, position.Y, Map.TILE_RESOLUTION, Map.TILE_RESOLUTION),
                sourceRectangle: new Rect(_currentFrame.X * spriteSheet.SourceResolution, _currentFrame.Y * spriteSheet.SourceResolution, spriteSheet.SourceResolution, spriteSheet.SourceResolution));
        }

        internal void Reset() {
            _currentFrameIndex = 0;
        }

        public SpriteAnimation(bool loops = true, double frameDurationInMilliseconds = 100) {
            _loops = loops;
            _frameDurationInMilliseconds = frameDurationInMilliseconds;
            _millisecondsSinceLastUpdate = 0;
        }

        public void Update(CanvasAnimatedUpdateEventArgs args) {
            _millisecondsSinceLastUpdate += args.Timing.ElapsedTime.TotalMilliseconds;
            if (_millisecondsSinceLastUpdate >= _frameDurationInMilliseconds) {
                _currentFrameIndex = _currentFrameIndex + 1;
                if (_currentFrameIndex >= Frames.Count) {
                    if (_loops) { _currentFrameIndex %= Frames.Count; }
                    else { _currentFrameIndex--; }
                }
                _millisecondsSinceLastUpdate -= _frameDurationInMilliseconds;
            }
        }

        public static SpriteAnimation Copy(SpriteAnimation animation) {
            bool loops = animation._loops;
            double frameDurationInMilliseconds = animation._frameDurationInMilliseconds;
            SpriteAnimation copy = new SpriteAnimation(loops, frameDurationInMilliseconds);
            foreach (SpriteAnimationFrame frame in animation.Frames) { copy.Frames.Add(SpriteAnimationFrame.Copy(frame)); }
            return copy;
        }
    }
}
