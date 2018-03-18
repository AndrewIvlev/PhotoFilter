using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PhotoFilter
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);

        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }

            }

            return resultImage;
        }
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    };
    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                                                255 - sourceColor.G,
                                                255 - sourceColor.B);
            return resultColor;
        }
    };
    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb((int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B),
                                               (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B),
                                               (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B));
            return resultColor;
        }
    };
    class Sepia : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = 44;
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(
                 Clamp((int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B + 2 * k), 0, 255),
                  Clamp((int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B + 0.5 * k), 0, 255),
                   Clamp((int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B - k), 0, 255)
                   );
            return resultColor;
        }
    };
    class Brightness : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int b = 66;
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(
                 Clamp(b + sourceColor.R, 0, 255),
                  Clamp(sourceColor.G + b, 0, 255),
                   Clamp(b + sourceColor.B, 0, 255)
                   );
            return resultColor;
        }
    };
    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    };
    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    };
    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    };
    class SobelFilter : MatrixFilter
    {
        public SobelFilter(bool f)
        {
            if (f)
                kernel = new float[,] {
                    { -1, -2, -1 },
                    { 0, 0, 0 },
                    { 1, 2, 1 }
                };
            else
                kernel = new float[,] {
                    { -1, 0, 1 },
                    { -2, 0, 2 },
                    { -1, 0, 1 }
                };
        }
    };
    class Prewitt : MatrixFilter
    {
        public Prewitt(bool f)
        {
            if (f)
                kernel = new float[,] {
                    { -1, -1, -1 },
                    { 0, 0, 0 },
                    { 1, 1, 1 }
                };
            else
                kernel = new float[,] {
                    { -1, 0, 1 },
                    { -1, 0, 1 },
                    { -1, 0, 1 }
                };
        }
    };
    class Embossing : MatrixFilter
    {
        public Embossing()
        {
            kernel = new float[,] {
                { 0, 1, 0 },
                { 1, 0, -1 },
                { 0, -1, 0 }
            };
        }
    };
    class Sharpen : MatrixFilter
    {
        public Sharpen()
        {
            kernel = new float[,] {
                { 0, -1, 0 },
                { -1, 5, -1 },
                { 0, -1, 0 }
            };
        }
    };
    class Sharp : MatrixFilter
    {
        public Sharp()
        {
            kernel = new float[,] {
                { -1, -1, -1 },
                { -1, 9, -1 },
                { -1, -1, -1 }
            };
        }
    };
    class MedianFilter : Filters
    {
        protected int radius;
        int colorArraySize;
        public MedianFilter(int _radius = 3)
        {
            radius = _radius;
            colorArraySize = (2 * radius + 1) * (2 * radius + 1);
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color[] ColorArray = new Color[colorArraySize];
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    ColorArray[(radius + dx) * (2 * radius + 1) + radius + dy] = sourceImage.GetPixel(Clamp(x + dx, 0, sourceImage.Width - 1), Clamp(y + dy, 0, sourceImage.Height - 1));
                }
            for (int i = 0; i < colorArraySize; i++)
                for (int j = i; j < colorArraySize; j++)
                {

                    int Intensity1 = (int)(0.36 * ColorArray[i].R + 0.53 * ColorArray[i].G + 0.11 * ColorArray[i].B);
                    int Intensity2 = (int)(0.36 * ColorArray[j].R + 0.53 * ColorArray[j].G + 0.11 * ColorArray[j].B);
                    if (Intensity2 < Intensity1)
                    {
                        Color tmp = ColorArray[i];
                        ColorArray[i] = ColorArray[j];
                        ColorArray[j] = tmp;
                    }
                }
            return ColorArray[colorArraySize / 2];
        }
    }
    class GlowingEdgesFilter : MatrixFilter
    {
        protected int radius;
        int colorArraySize;

        protected float[,] GX = new float[,] {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        },
            GY = new float[,] {
            { -1, -2, -1 },
            { 0, 0, 0 },
            { 1, 2, 1 }
        };
        public GlowingEdgesFilter(int _radius = 3)
        {
            radius = _radius;
            colorArraySize = (2 * radius + 1) * (2 * radius + 1);
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color[] ColorArray = new Color[colorArraySize];
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    ColorArray[(radius + dx) * (2 * radius + 1) + radius + dy] = sourceImage.GetPixel(Clamp(x + dx, 0, sourceImage.Width - 1), Clamp(y + dy, 0, sourceImage.Height - 1));
                }
            for (int i = 0; i < colorArraySize; i++)
                for (int j = i; j < colorArraySize; j++)
                {

                    int Intensity1 = (int)(0.36 * ColorArray[i].R + 0.53 * ColorArray[i].G + 0.11 * ColorArray[i].B);
                    int Intensity2 = (int)(0.36 * ColorArray[j].R + 0.53 * ColorArray[j].G + 0.11 * ColorArray[j].B);
                    if (Intensity2 < Intensity1)
                    {
                        Color tmp = ColorArray[i];
                        ColorArray[i] = ColorArray[j];
                        ColorArray[j] = tmp;
                    }
                }

            Bitmap temp = sourceImage;
            temp.SetPixel(x, y, ColorArray[colorArraySize / 2]);
            kernel = GX;
            Color colorX = base.calculateNewPixelColor(temp, x, y);

            kernel = GY;
            Color colorY = base.calculateNewPixelColor(temp, x, y);

            float rx = colorX.R, gx = colorX.G, bx = colorX.B,
                ry = colorY.R, gy = colorY.G, by = colorY.B;

            int result = Clamp(
                (int)Math.Sqrt(rx * rx + ry * ry + gx * gx + gy * gy + bx * bx + by * by),
                0,
                255
                );

            return Color.FromArgb(result, result, result);
        }
    }
    class ShiftFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            if (x < sourceImage.Width - 50) return sourceImage.GetPixel(x + 50, y);
            return Color.FromArgb(0, 0, 0);
        }
    }

    class RotationFilter : Filters
    {
        double angle;
        int x0, y0;
        public RotationFilter(int tx0 = 0, int ty0 = 0, double tAngle = Math.PI / 4)
        {
            angle = tAngle;
            x0 = tx0;
            y0 = ty0;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color result;
            double xr = (x - x0) * Math.Cos(angle) - (y - y0) * Math.Sin(angle) + x0;
            double yr = (x - x0) * Math.Sin(angle) + (y - y0) * Math.Cos(angle) + y0;

            if ((xr < sourceImage.Width) &&
                (xr > 0) &&
                (yr < sourceImage.Height) &&
                (yr > 0)) result = sourceImage.GetPixel((int)xr, (int)yr);
            else result = Color.FromArgb(0, 0, 0);

            return result;
        }
    }
    class Waves1Filter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            double resultx = x + 20 * Math.Sin(2 * Math.PI * y / 60);

            if ((resultx < sourceImage.Width) && (resultx > -1)) return sourceImage.GetPixel((int)resultx, y);
            else return sourceImage.GetPixel((int)Math.Abs(resultx) % sourceImage.Width, y);
        }
    }

    class Waves2Filter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            double resultx = x + 20 * Math.Sin(2 * Math.PI * x / 30);

            if ((resultx < sourceImage.Width) && (resultx > -1)) return sourceImage.GetPixel((int)resultx, y);
            else return sourceImage.GetPixel((int)Math.Abs(resultx) % sourceImage.Width, y);
        }
    }
    class GlassFilter : Filters
    {
        Random rand;
        public GlassFilter()
        {
            rand = new Random();
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return sourceImage.GetPixel(
                Clamp((int)(x + (rand.NextDouble() - 0.5) * 10), 0, sourceImage.Width - 1),
                Clamp((int)(y + (rand.NextDouble() - 0.5) * 10), 0, sourceImage.Height - 1)
                );
        }
    }
    class MotionBlurFilter : MatrixFilter
    {
        const int n = 7;
        public MotionBlurFilter()
        {
            kernel = new float[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (i == j) kernel[i, j] = 1.0f / (float)n;
                    else kernel[i, j] = 0.0f;
        }
    }
    class LiniarStretchingFilter : Filters
    {
        int Rmax = 0, Rmin = 255, Gmax = 0, Gmin = 255, Bmax = 0, Bmin = 255;
        int difR = 255, difG = 255, difB = 255;

        public LiniarStretchingFilter(Bitmap src)
        {
            int ImWidth = src.Width, ImHeight = src.Height;
            int tmpR, tmpG, tmpB;

            for (int i = 0; i < ImWidth; i++)
                for (int j = 0; j < ImHeight; j++)
                {
                    tmpR = src.GetPixel(i, j).R;
                    if (tmpR > Rmax) Rmax = tmpR;
                    if (tmpR < Rmin) Rmin = tmpR;

                    tmpG = src.GetPixel(i, j).G;
                    if (tmpG > Gmax) Gmax = tmpG;
                    if (tmpG < Gmin) Gmin = tmpG;

                    tmpB = src.GetPixel(i, j).B;
                    if (tmpB > Bmax) Bmax = tmpB;
                    if (tmpB < Bmin) Bmin = tmpB;
                }
            difR = Rmax - Rmin;
            difG = Gmax - Gmin;
            difB = Bmax - Bmin;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color tmp = sourceImage.GetPixel(x, y);

            return Color.FromArgb(
                Clamp((tmp.R - Rmin) * (255 / difR), 0, 255),
                Clamp((tmp.G - Gmin) * (255 / difG), 0, 255),
                Clamp((tmp.B - Bmin) * (255 / difB), 0, 255)
                );
        }
    }

    class GreyWorldFilter : Filters
    {
        double Ravg = 128, Gavg = 128, Bavg = 128;
        double Avg = 128;
        double Rcoeff = 1, Gcoeff = 1, Bcoeff = 1;
        public GreyWorldFilter(Bitmap sourceImage)
        {
            double Rsum = 0, Gsum = 0, Bsum = 0;
            double ImageSize = sourceImage.Width * sourceImage.Height;

            for (int i = 0; i < sourceImage.Height; i++)
            {
                for (int j = 0; j < sourceImage.Width; j++)
                {
                    Rsum += sourceImage.GetPixel(j, i).R;
                    Gsum += sourceImage.GetPixel(j, i).G;
                    Bsum += sourceImage.GetPixel(j, i).B;
                }
            }
            Ravg = Rsum / ImageSize;
            Gavg = Gsum / ImageSize;
            Bavg = Bsum / ImageSize;

            Avg = (Ravg + Gavg + Bavg) / 3.0;

            Rcoeff = Ravg / Avg;
            Bcoeff = Bavg / Avg;
            Gcoeff = Gavg / Avg;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return Color.FromArgb(
                Clamp((int)(sourceImage.GetPixel(x, y).R * Rcoeff), 0, 255),
                Clamp((int)(sourceImage.GetPixel(x, y).G * Gcoeff), 0, 255),
                Clamp((int)(sourceImage.GetPixel(x, y).B * Bcoeff), 0, 255)
                );
        }
    }
    class DilationOperation : Filters
    {
        protected int[,] kernel;
        private int radius;
        public DilationOperation(int[,] _kernel = null)
        {
            if (_kernel == null)
            {
                kernel = new int[,] {
                { 0, 0, 1, 1, 1, 0, 0},
                { 0, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 0, 1, 1, 1, 1, 1, 0},
                { 0, 0, 1, 1, 1, 0, 0}
                };
            }
            else kernel = _kernel;

            radius = kernel.GetLength(0) / 2;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int max = 0, indexX = 0, indexY = 0, pixel = 0;
            int ImageWidth = sourceImage.Width, ImageHeight = sourceImage.Height;
            for (int k = -radius; k <= radius; k++)
                for (int l = -radius; l <= radius; l++)
                {
                    pixel = sourceImage.GetPixel(Clamp(x + k, 0, ImageWidth - 1), Clamp(y + l, 0, ImageHeight - 1)).R;
                    indexX = radius + k;
                    indexY = radius + l;

                    if ((kernel[indexX, indexY] == 1) && (pixel > max)) max = pixel;
                }
            return Color.FromArgb(max, max, max);
        }
    }

    class ErosionOperation : Filters
    {
        protected int[,] kernel;
        private int radius;
        public ErosionOperation(int[,] _kernel = null)
        {
            if (_kernel == null)
            {
                kernel = new int[,] {
                { 0, 0, 1, 1, 1, 0, 0},
                { 0, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 0, 1, 1, 1, 1, 1, 0},
                { 0, 0, 1, 1, 1, 0, 0}
                };
            }
            else kernel = _kernel;

            radius = kernel.GetLength(0) / 2;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int min = 255, indexX = 0, indexY = 0, pixel = 0;
            int ImageWidth = sourceImage.Width, ImageHeight = sourceImage.Height;
            for (int k = -radius; k <= radius; k++)
                for (int l = -radius; l <= radius; l++)
                {
                    pixel = sourceImage.GetPixel(Clamp(x + k, 0, ImageWidth - 1), Clamp(y + l, 0, ImageHeight - 1)).R;
                    indexX = radius + k;
                    indexY = radius + l;

                    if ((kernel[indexX, indexY] == 1) && (pixel < min)) min = pixel;
                }
            return Color.FromArgb(min, min, min);
        }

    }

    class Opening : Filters
    {
        protected int[,] kernel;
        private int radius;
        public Opening(int[,] _kernel = null)
        {
            if (_kernel == null)
            {
                kernel = new int[,] {
                { 0, 0, 1, 1, 1, 0, 0},
                { 0, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 0, 1, 1, 1, 1, 1, 0},
                { 0, 0, 1, 1, 1, 0, 0}
                };
            }
            else kernel = _kernel;

            radius = kernel.GetLength(0) / 2;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int min = 255, indexX = 0, indexY = 0, pixel = 0;
            int ImageWidth = sourceImage.Width, ImageHeight = sourceImage.Height;
            Bitmap tmpImage = sourceImage;
            int max = 0;

            for (int k = -radius; k <= radius; k++)
                for (int l = -radius; l <= radius; l++)
                {
                    pixel = tmpImage.GetPixel(Clamp(x + k, 0, ImageWidth - 1), Clamp(y + l, 0, ImageHeight - 1)).R;
                    indexX = radius + k;
                    indexY = radius + l;

                    if ((kernel[indexX, indexY] == 1) && (pixel < min)) min = pixel;
                }

            tmpImage.SetPixel(x, y, Color.FromArgb(min, min, min));

            for (int k = -radius; k <= radius; k++)
                for (int l = -radius; l <= radius; l++)
                {
                    pixel = tmpImage.GetPixel(Clamp(x + k, 0, ImageWidth - 1), Clamp(y + l, 0, ImageHeight - 1)).R;
                    indexX = radius + k;
                    indexY = radius + l;

                    if ((kernel[indexX, indexY] == 1) && (pixel > max)) max = pixel;
                }

            return Color.FromArgb(max, max, max);
        }
    }

    class Closing : Filters
    {
        protected int[,] kernel;
        private int radius;
        public Closing(int[,] _kernel = null)
        {
            if (_kernel == null)
            {
                kernel = new int[,] {
                { 0, 0, 1, 1, 1, 0, 0},
                { 0, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 0, 1, 1, 1, 1, 1, 0},
                { 0, 0, 1, 1, 1, 0, 0}
                };
            }
            else kernel = _kernel;

            radius = kernel.GetLength(0) / 2;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int min = 255, indexX = 0, indexY = 0, pixel = 0;
            int ImageWidth = sourceImage.Width, ImageHeight = sourceImage.Height;
            Bitmap tmpImage = sourceImage;
            int max = 0;

            for (int k = -radius; k <= radius; k++)
                for (int l = -radius; l <= radius; l++)
                {
                    pixel = tmpImage.GetPixel(Clamp(x + k, 0, ImageWidth - 1), Clamp(y + l, 0, ImageHeight - 1)).R;
                    indexX = radius + k;
                    indexY = radius + l;

                    if ((kernel[indexX, indexY] == 1) && (pixel > max)) max = pixel;
                }

            tmpImage.SetPixel(x, y, Color.FromArgb(max, max, max));

            for (int k = -radius; k <= radius; k++)
                for (int l = -radius; l <= radius; l++)
                {
                    pixel = tmpImage.GetPixel(Clamp(x + k, 0, ImageWidth - 1), Clamp(y + l, 0, ImageHeight - 1)).R;
                    indexX = radius + k;
                    indexY = radius + l;

                    if ((kernel[indexX, indexY] == 1) && (pixel < min)) min = pixel;
                }

            return Color.FromArgb(min, min, min);
        }
    }

    class TopHatFilter : Filters
    {
        protected int[,] kernel;
        private int radius;
        public TopHatFilter(int[,] _kernel = null)
        {
            if (_kernel == null)
            {
                kernel = new int[,] {
                { 0, 0, 1, 1, 1, 0, 0},
                { 0, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 1, 1, 1, 1, 1, 1, 1},
                { 0, 1, 1, 1, 1, 1, 0},
                { 0, 0, 1, 1, 1, 0, 0}
                };
            }
            else kernel = _kernel;

            radius = kernel.GetLength(0) / 2;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int min = 255, indexX = 0, indexY = 0, pixel = 0;
            int ImageWidth = sourceImage.Width, ImageHeight = sourceImage.Height;
            Bitmap tmpImage = sourceImage;
            int max = 0;

            for (int k = -radius; k <= radius; k++)
                for (int l = -radius; l <= radius; l++)
                {
                    pixel = tmpImage.GetPixel(Clamp(x + k, 0, ImageWidth - 1), Clamp(y + l, 0, ImageHeight - 1)).R;
                    indexX = radius + k;
                    indexY = radius + l;

                    if ((kernel[indexX, indexY] == 1) && (pixel > max)) max = pixel;
                }

            tmpImage.SetPixel(x, y, Color.FromArgb(max, max, max));

            for (int k = -radius; k <= radius; k++)
                for (int l = -radius; l <= radius; l++)
                {
                    pixel = tmpImage.GetPixel(Clamp(x + k, 0, ImageWidth - 1), Clamp(y + l, 0, ImageHeight - 1)).R;
                    indexX = radius + k;
                    indexY = radius + l;

                    if ((kernel[indexX, indexY] == 1) && (pixel < min)) min = pixel;
                }
            return Color.FromArgb(
                Clamp(sourceImage.GetPixel(x, y).R - min, 0, 255),
                Clamp(sourceImage.GetPixel(x, y).G - min, 0, 255),
                Clamp(sourceImage.GetPixel(x, y).B - min, 0, 255)
                );
        }
    }
}
