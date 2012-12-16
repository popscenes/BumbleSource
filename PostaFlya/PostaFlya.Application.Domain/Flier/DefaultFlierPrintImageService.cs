using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Content;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.Flier
{
    public class DefaultFlierPrintImageService : FlierPrintImageServiceInterface
    {
        private readonly BlobStorageInterface _blobStorage;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly QrCodeServiceInterface _qrCodeService;

        public DefaultFlierPrintImageService([ImageStorage]BlobStorageInterface blobStorage
            , GenericQueryServiceInterface queryService, QrCodeServiceInterface qrCodeService)
        {
            _blobStorage = blobStorage;
            _queryService = queryService;
            _qrCodeService = qrCodeService;
        }

        public Image GetPrintImageForFlier(string flierId)
        {
            var flier = _queryService.FindById<PostaFlya.Domain.Flier.Flier>(flierId);
            if (flier == null || !flier.Image.HasValue)
                return null;

            var imgData = _blobStorage.GetBlob(flier.Image.Value + ImageUtil.GetIdFileExtension());
            if(imgData == null || imgData.Length == 0)
                return null;

            using (var ms = new MemoryStream(imgData))
            {
                var srcimage = Image.FromStream(ms);
                return CreateFlierImageWithTearOffs(srcimage,
                                                    GetSoureUrl(),
                                                    flier.FriendlyId, 
                                                    Properties.Resources.PostaLogo, _qrCodeService);
            }        
        }

        public static Image CreateFlierImageWithTearOffs(Image srcimage
                                                         , string baseurl, string idtext, Image logo, QrCodeServiceInterface qrCodeService)
        {
            var width = ImageUtil.A4300DpiSize.Width;
            var height = ImageUtil.A4300DpiSize.Height;

            const int qrcodewidthheight = 480;
            const int logowidth = 480;
            const int logoheight = 200;

            const int tearoffheight = qrcodewidthheight + logoheight;
            const int tearoffwidth = 200;
            const int qrcodetearoffwidth = 200;
            const int tearofftextheight = 480;


            var imgToDisp = new HashSet<IDisposable>();
            if (srcimage.Width > srcimage.Height)
                srcimage.RotateFlip(RotateFlipType.Rotate90FlipNone);

            var target = new Bitmap(width, height);

            var resized = new Bitmap(srcimage, width, height - tearoffheight);
            imgToDisp.Add(resized);
            var qrimage = qrCodeService.QrCodeImg(baseurl + idtext, qrcodewidthheight, 1);
            imgToDisp.Add(qrimage);
            var qrimagetear = qrCodeService.QrCodeImg(baseurl + idtext, qrcodetearoffwidth, 2);
            imgToDisp.Add(qrimagetear);
            var logoResized = new Bitmap(logo, logowidth, logoheight);
            imgToDisp.Add(logoResized);

            var graphics = Graphics.FromImage(target);
            graphics.FillRectangle(Brushes.White, -1, -1, target.Width + 1, target.Height + 1);
            graphics.DrawImage(resized, 0, 0);
            graphics.DrawImage(logoResized, 0, height - tearoffheight);
            graphics.DrawImage(qrimage, 0, height - qrcodewidthheight);

            var font = new Font("Arial", 22, FontStyle.Bold);
            imgToDisp.Add(font);

            var blackPen = new Pen(Color.Black, 2);
            imgToDisp.Add(blackPen);

            for (var i = 0; i < 10; i++)
            {
                var xOff = qrcodewidthheight + (i * tearoffwidth);
                graphics.DrawImage(qrimagetear, xOff, height - qrcodetearoffwidth);

                const int lineheight = tearoffwidth / 4;
                
                //tear off border
                graphics.DrawRectangle(blackPen, xOff, height - tearoffheight, tearoffwidth, tearoffheight);
                
                //line 1
                graphics.TranslateTransform(xOff + (lineheight * 2), height - tearoffheight);
                graphics.RotateTransform(90);
                SizeF textSize = graphics.MeasureString(baseurl, font);
                var scale = tearofftextheight / textSize.Width;
                if (scale < 1)
                    graphics.ScaleTransform(scale, scale);
                var xoffset = Math.Max((tearofftextheight - textSize.Width) / 2, 0.0F);
                graphics.DrawString(baseurl, font, Brushes.Black, xoffset, -textSize.Height);
                graphics.ResetTransform();

                //line 2
                graphics.TranslateTransform(xOff + (lineheight), height - tearoffheight);
                graphics.RotateTransform(90);
                textSize = graphics.MeasureString(idtext, font);
                scale = tearofftextheight / textSize.Width;
                if (scale < 1)
                    graphics.ScaleTransform(scale, scale);
                xoffset = Math.Max((tearofftextheight - textSize.Width) / 2, 0.0F);
                graphics.DrawString(idtext, font, Brushes.Black, xoffset, -textSize.Height);
                graphics.ResetTransform();

            }

            foreach (var iDisposable in imgToDisp)
            {
                iDisposable.Dispose();
            }

            return target;
        }

        private static string GetSoureUrl()
        {
            var ret = Config.Instance.GetSetting("QrUrl");

            if (!string.IsNullOrWhiteSpace(ret) && ret.Equals("uselocal"))
            {
                var ipadd = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                return "http://" + ipadd.ToString() + "/";
            }
                
            if (string.IsNullOrWhiteSpace(ret))
                return "http://www.postaflya.com/";
            return (!ret.EndsWith("/")) ? ret + "/" : ret;
        }
    }
}