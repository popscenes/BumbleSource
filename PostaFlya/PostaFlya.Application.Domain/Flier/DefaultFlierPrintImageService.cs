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

        public Image GetPrintImageForFlierWithTearOffs(string flierId)
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

        public Image GetQrCodeImageForFlier(string flierId)
        {
            var flier = _queryService.FindById<PostaFlya.Domain.Flier.Flier>(flierId);
            if (flier == null || !flier.Image.HasValue)
                return null;

            return CreateQrCodeImageForFlier(
                                    GetSoureUrl(),
                                    flier.FriendlyId,
                                    Properties.Resources.PostaLogo,
                                    _qrCodeService, Qrcodewidthheight, Logoheight);
        }

        public Image GetPrintImageForFlier(string flierId, FlierPrintImageServiceQrLocation qrLocation)
        {
            var flier = _queryService.FindById<PostaFlya.Domain.Flier.Flier>(flierId);
            if (flier == null || !flier.Image.HasValue)
                return null;

            var imgData = _blobStorage.GetBlob(flier.Image.Value + ImageUtil.GetIdFileExtension());
            if (imgData == null || imgData.Length == 0)
                return null;

            using (var ms = new MemoryStream(imgData))
            {
                var srcimage = Image.FromStream(ms);
                return CreateFlierImage(srcimage, GetSoureUrl(),
                                            flier.FriendlyId,
                                            Properties.Resources.PostaLogo, _qrCodeService, qrLocation);
            }  
        }

        const int Qrcodewidthheight = 480;
        const int Logoheight = 200;

        public static Image CreateFlierImage(Image srcimage, string baseurl, string idtext, Bitmap logo, QrCodeServiceInterface qrCodeService, FlierPrintImageServiceQrLocation qrLocation)
        {
            
            var width = ImageUtil.A4300DpiSize.Width;
            var height = ImageUtil.A4300DpiSize.Height;
            const int qrcodewidthheight = Qrcodewidthheight;
            const int logoheight = Logoheight;

            if (srcimage.Width > srcimage.Height)
            {
                width = ImageUtil.A4300DpiSize.Height;
                height = ImageUtil.A4300DpiSize.Width;
            }
            
            const int qrPadding = 10;
            int xloc = qrPadding, yloc = qrPadding; 
            switch (qrLocation)
            {
                case FlierPrintImageServiceQrLocation.TopLeft:
                    break;
                case FlierPrintImageServiceQrLocation.TopRight:
                    xloc = width - qrPadding - qrcodewidthheight;
                    break;
                case FlierPrintImageServiceQrLocation.BottomLeft:
                    yloc = height - qrPadding - qrcodewidthheight - logoheight;
                    break;
                case FlierPrintImageServiceQrLocation.BottomRight:
                    yloc = height - qrPadding - qrcodewidthheight - logoheight;
                    xloc = width - qrPadding - qrcodewidthheight;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("qrLocation");
            }
                
            var target = new Bitmap(srcimage, width, height);

            var imgToDisp = new HashSet<IDisposable>();
            var qrimage = CreateQrCodeImageForFlier(baseurl, idtext, logo, qrCodeService, qrcodewidthheight, 
                                        logoheight);
            imgToDisp.Add(qrimage);

            var graphics = Graphics.FromImage(target);
            imgToDisp.Add(graphics);
            graphics.DrawImage(qrimage, xloc, yloc);

            foreach (var iDisposable in imgToDisp)
            {
                iDisposable.Dispose();
            }

            return target;
        }


        public static Image CreateQrCodeImageForFlier(string baseurl
            , string idtext, Image logo, QrCodeServiceInterface qrCodeService
            , int qrcodewidthheight, int logoheight)
        {
            var imgToDisp = new HashSet<IDisposable>();

            var qrimage = qrCodeService.QrCodeImg(baseurl + idtext, qrcodewidthheight, 1);
            imgToDisp.Add(qrimage);
            var logoResized = new Bitmap(logo, qrcodewidthheight, logoheight);
            imgToDisp.Add(logoResized);

            var target = new Bitmap(qrcodewidthheight, qrcodewidthheight + logoheight);

            var graphics = Graphics.FromImage(target);
            imgToDisp.Add(graphics);

            graphics.DrawImage(logoResized, 0, 0);
            graphics.DrawImage(qrimage, 0, logoheight);

            foreach (var iDisposable in imgToDisp)
            {
                iDisposable.Dispose();
            }

            return target;
        }

        public static Image CreateFlierImageWithTearOffs(Image srcimage
                                                         , string baseurl, string idtext, Image logo, QrCodeServiceInterface qrCodeService)
        {
            var width = ImageUtil.A4300DpiSize.Width;
            var height = ImageUtil.A4300DpiSize.Height;

            const int qrcodewidthheight = Qrcodewidthheight;
            //const int logowidth = qrcodewidthheight;
            const int logoheight = Logoheight;

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

            var qrimage = CreateQrCodeImageForFlier(baseurl, idtext, logo, qrCodeService, qrcodewidthheight, 
                                                    logoheight);
            imgToDisp.Add(qrimage);
            
            var qrimagetear = qrCodeService.QrCodeImg(baseurl + idtext, qrcodetearoffwidth, 2);
            imgToDisp.Add(qrimagetear);


            var graphics = Graphics.FromImage(target);
            imgToDisp.Add(graphics);
            graphics.FillRectangle(Brushes.White, -1, -1, target.Width + 1, target.Height + 1);
            graphics.DrawImage(resized, 0, 0);
            graphics.DrawImage(qrimage, 0, height - tearoffheight);

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