using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace procedural_map {
    class SpriteSheet {
        private CanvasBitmap _image;
        public CanvasBitmap Image { get { return _image; } }

        private int _sourceResolution;
        public int SourceResolution { get { return _sourceResolution; } }

        private SpriteSheet() { }
        public SpriteSheet(CanvasBitmap image, int sourceResolution) {
            _image = image;
            _sourceResolution = sourceResolution;
        }
    }
}
