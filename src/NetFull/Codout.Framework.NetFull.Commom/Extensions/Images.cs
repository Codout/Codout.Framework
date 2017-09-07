using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Codout.Framework.NetFull.Commom.Extensions
{
    /// <summary>
    /// Extensões comuns para tipos relacionadas a imagens.
    /// </summary>
    public static class Images
    {
        #region CreateThumbnail
        /// <summary>
        /// Cria uma miniatura da imagem
        /// </summary>
        /// <param name="image">Imagem original</param>
        /// <param name="width">A (máxima) largura.</param>
        /// <returns>Retorna a imagem</returns>
        public static Image CreateThumbnail(this Image image, int width)
        {
            //overload for just maximum width
            return image.CreateThumbnail(width, 0);
        }
        #endregion

        #region CreateThumbnail
        /// <summary>
        /// Cria uma miniatura da imagem
        /// </summary>
        /// <param name="image">Imagem original</param>
        /// <param name="maxWidth">A (máxima) largura.</param>
        /// <param name="maxHeight">A (máxima) altura.</param>
        /// <returns>Retorna a imagem</returns>
        public static Image CreateThumbnail(this Image image, int maxWidth, int maxHeight)
        {
            double centerWidth = 0;
            double centerHeight = 0;
            double width = Convert.ToDouble(image.Width);
            double height = Convert.ToDouble(image.Height);
            double widthFinal;
            double heightFinal;
            double newWidth = width;
            double newHeight = height;

            double ratioWidth = maxWidth > 0 ? maxWidth / width : 0;
            double ratioHeight = maxHeight > 0 ? (maxHeight / height) : 0;
            double ratio = Math.Max(ratioWidth, ratioHeight);

            // Se a imagem é maior que o permitido, encolhe ela!
            if (ratio < 1)
            {
                newWidth = Math.Floor(Convert.ToDouble(ratio * width));
                newHeight = Math.Floor(Convert.ToDouble(ratio * height));
                widthFinal = (maxWidth != 0 && newWidth > maxWidth) ? maxWidth : newWidth;
                heightFinal = (maxHeight != 0 && newHeight > maxHeight) ? maxHeight : newHeight;
                if (maxWidth != 0 && (newWidth - maxWidth) != 0) centerWidth = (maxWidth - newWidth) / 2;
                if (maxHeight != 0 && (newHeight - maxHeight) != 0) centerHeight = (maxHeight - newHeight) / 2;
            }
            else
            {
                widthFinal = newWidth;
                heightFinal = newHeight;
                centerWidth = 0;
                centerHeight = 0;
            }

            //Criando a imagem  no tamanho passado pelo parametro com o mesmo formado de pixel da imagem original
            Image oImgFinal = new Bitmap(Convert.ToInt32(widthFinal), Convert.ToInt32(heightFinal), image.PixelFormat);

            //Transformando o fundo em um Gráfico
            Graphics graphic = Graphics.FromImage(oImgFinal);
            // Alteramos algumas propriedades do objeto oGraphic para melhorar a qualidade final da imagem.
            graphic.CompositingQuality = CompositingQuality.HighQuality;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.FillRectangle(Brushes.White, 0, 0, Convert.ToInt32(widthFinal), Convert.ToInt32(heightFinal));

            //Redimencionando Imagem original
            var finalSize = new Size(Convert.ToInt32(newWidth), Convert.ToInt32(newHeight));
            //Criando um fundo para colocar a imagem original 
            var rectangle = new Rectangle(Convert.ToInt32(centerWidth), Convert.ToInt32(centerHeight), finalSize.Width, finalSize.Height);
            graphic.FillRectangle(Brushes.White, rectangle);

            // redimencionando a imagem original e o fundo dentro da imagem nova.
            graphic.DrawImage(image, rectangle);

            //E por fim produzimos a saída da página como uma imagem JPG. 
            image.Dispose();

            return oImgFinal;
        }
        #endregion

        #region Crop
        /// <summary>
        /// Método de corte de uma imagem
        /// </summary>
        /// <param name="image">Imagem a ser recortada</param>
        /// <param name="width">Altura</param>
        /// <param name="height">Largura</param>
        /// <param name="x">Coordenada X</param>
        /// <param name="y">Coordenada Y</param>
        /// <returns></returns>
        public static Image Crop(this Image image, int width, int height, int x, int y)
        {
            Image bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            Graphics graphic = Graphics.FromImage(bmp);
            graphic.CompositingQuality = CompositingQuality.HighQuality;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.FillRectangle(Brushes.White, 0, 0, Convert.ToInt32(width), Convert.ToInt32(height));

            graphic.DrawImage(image, new Rectangle(0, 0, width, height), x, y, width, height, GraphicsUnit.Pixel);

            image.Dispose();
            graphic.Dispose();

            return bmp;
        }
        #endregion

        #region ToImage
        /// <summary>
        /// Converte um byte[] em uma Imagem
        /// </summary>
        /// <param name="image">Byte array contendo uma imagem.</param>
        /// <returns>Uma imagem se a conversão ocorrer com sucesso.</returns>
        public static Image ToImage(this byte[] image)
        {
            if (image == null)
                return new Bitmap(1, 1);

            return new ImageConverter().ConvertFrom(image) as Image;
        }
        #endregion
    }
}
