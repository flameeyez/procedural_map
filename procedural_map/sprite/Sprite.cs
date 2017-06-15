using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Foundation;
using Windows.System;
using Microsoft.Graphics.Canvas;

namespace procedural_map {
    class Sprite {
        //public SpriteAnimationSet SpriteAnimationSet { get; set; }
        public PointInt AbsolutePosition { get; set; }
        public PointInt ScreenPosition { get { return new PointInt(AbsolutePosition.X - Camera.PositionX, AbsolutePosition.Y - Camera.PositionY); } }
        private SpriteSheet _spriteSheet;
        private SPRITE_ANIMATION _currentAnimationState;
        private SpriteAnimation _currentAnimation;
        //private int _frameIndex = 0;

        public bool IsOnScreen {
            get {
                if (AbsolutePosition.X < Camera.PositionX - Map.TILE_RESOLUTION) { return false; }
                if (AbsolutePosition.X > Camera.PositionX + Statics.ClientWidth) { return false; }
                if (AbsolutePosition.Y < Camera.PositionY - Map.TILE_RESOLUTION) { return false; }
                if (AbsolutePosition.Y > Camera.PositionY + Statics.ClientHeight) { return false; }
                return true;
            }
        }

        private Queue<SPRITE_ANIMATION> _stateQueue = new Queue<SPRITE_ANIMATION>();

        private Sprite() { }
        public Sprite(CanvasBitmap image, int imageResolution, PointInt absolutePosition) {
            AbsolutePosition = absolutePosition;
            _spriteSheet = new SpriteSheet(image, imageResolution);

            _currentAnimationState = SPRITE_ANIMATION.IDLE_DOWN;
            _currentAnimation = SpriteAnimationDefinitions.Copy(_currentAnimationState);
        }

        public void Draw(CanvasAnimatedDrawEventArgs args) {
            if (IsOnScreen) {
                _currentAnimation.Draw(_spriteSheet, ScreenPosition, args);
            }
        }
        public void Update(CanvasAnimatedUpdateEventArgs args) {
            UpdatePosition();
            UpdateAnimation(args);
        }

        private void UpdatePosition() {
            switch (_currentAnimationState) {
                case SPRITE_ANIMATION.WALK_DOWN:
                    AbsolutePosition.Y += 1;
                    if (AbsolutePosition.Y % Map.TILE_RESOLUTION == 0) { SetNextState(); }
                    break;
                case SPRITE_ANIMATION.WALK_UP:
                    AbsolutePosition.Y -= 1;
                    if (AbsolutePosition.Y % Map.TILE_RESOLUTION == 0) { SetNextState(); }
                    break;
                case SPRITE_ANIMATION.WALK_LEFT:
                    AbsolutePosition.X -= 1;
                    if (AbsolutePosition.X % Map.TILE_RESOLUTION == 0) { SetNextState(); }
                    break;
                case SPRITE_ANIMATION.WALK_RIGHT:
                    AbsolutePosition.X += 1;
                    if (AbsolutePosition.X % Map.TILE_RESOLUTION == 0) { SetNextState(); }
                    break;
                case SPRITE_ANIMATION.IDLE_DOWN:
                case SPRITE_ANIMATION.IDLE_UP:
                case SPRITE_ANIMATION.IDLE_LEFT:
                case SPRITE_ANIMATION.IDLE_RIGHT:
                    if (_stateQueue.Count > 0) { SetNextState(); }
                    else if (Random.Next(100) == 0) { GenerateRandomQueue(); }
                    break;
            }
        }

        private void UpdateAnimation(CanvasAnimatedUpdateEventArgs args) {
            _currentAnimation.Update(args);
        }

        internal void HandleKeyboard(HashSet<VirtualKey> keysDown) {
            //if (keysDown.Contains(VirtualKey.Down)) { SetAnimation(SPRITE_ANIMATION.WALK_DOWN); }
            //else if (keysDown.Contains(VirtualKey.Up))
            //{
            //    if (Position.Y > 0) { SetAnimation(SPRITE_ANIMATION.WALK_UP); }
            //    else { SetAnimation(SPRITE_ANIMATION.IDLE_UP); }
            //}
            //else if (keysDown.Contains(VirtualKey.Left))
            //{
            //    if (Position.X > 0) { SetAnimation(SPRITE_ANIMATION.WALK_LEFT); }
            //    else { SetAnimation(SPRITE_ANIMATION.IDLE_LEFT); }
            //}
            //else if (keysDown.Contains(VirtualKey.Right)) { SetAnimation(SPRITE_ANIMATION.WALK_RIGHT); }
        }

        public void SetAnimation(SPRITE_ANIMATION animation) {
            _currentAnimationState = animation;
            _currentAnimation = SpriteAnimationDefinitions.Copy(_currentAnimationState);
        }

        private void GenerateRandomQueue() {
            for (int i = 0; i < 10; i++) {
                switch (Random.Next(4)) {
                    case 0:
                        _stateQueue.Enqueue(SPRITE_ANIMATION.WALK_RIGHT);
                        break;
                    case 1:
                        _stateQueue.Enqueue(SPRITE_ANIMATION.WALK_LEFT);
                        break;
                    case 2:
                        _stateQueue.Enqueue(SPRITE_ANIMATION.WALK_DOWN);
                        break;
                    case 3:
                        _stateQueue.Enqueue(SPRITE_ANIMATION.WALK_UP);
                        break;
                    default:
                        break;
                }
            }
        }

        private void SetNextState() {
            if (_stateQueue.Count > 0) {
                SPRITE_ANIMATION nextState = _stateQueue.Dequeue();
                if (nextState != _currentAnimationState) { SetAnimation(nextState); }
            }
            else {
                switch (_currentAnimationState) {
                    case SPRITE_ANIMATION.WALK_DOWN:
                        SetAnimation(SPRITE_ANIMATION.IDLE_DOWN);
                        break;
                    case SPRITE_ANIMATION.WALK_UP:
                        SetAnimation(SPRITE_ANIMATION.IDLE_UP);
                        break;
                    case SPRITE_ANIMATION.WALK_LEFT:
                        SetAnimation(SPRITE_ANIMATION.IDLE_LEFT);
                        break;
                    case SPRITE_ANIMATION.WALK_RIGHT:
                        SetAnimation(SPRITE_ANIMATION.IDLE_RIGHT);
                        break;
                }
            }
        }
    }
}
