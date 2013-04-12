using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using PostaFlya.Domain.Flier.Analytic;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Content;
using Website.Application.Extension.Content;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.Flier
{
    public class DefaultFlierPrintImageService : FlierPrintImageServiceInterface
    {
        private readonly BlobStorageInterface _blobStorage;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly QrCodeServiceInterface _qrCodeService;
        private readonly BlobStorageInterface _appStorage;

        public DefaultFlierPrintImageService([ImageStorage]BlobStorageInterface blobStorage
            , GenericQueryServiceInterface queryService, QrCodeServiceInterface qrCodeService,
            [ApplicationStorage]BlobStorageInterface appStorage)
        {
            _blobStorage = blobStorage;
            _queryService = queryService;
            _qrCodeService = qrCodeService;
            _appStorage = appStorage;
        }


        public Image GetPrintImageForFlierWithTearOffs(string flierId)
        {
            var flier = _queryService.FindById<PostaFlya.Domain.Flier.Flier>(flierId);
            if (flier == null || !flier.Image.HasValue)
                return null;

            var ret = GetBlobCache("imagewithtabs", flier.Image.ToString(), flier.Id, _appStorage);
            if (ret != null)
                return ret;

            var imgData = _blobStorage.GetBlob(flier.Image.Value + ImageUtil.GetIdFileExtension());
            if(imgData == null || imgData.Length == 0)
                return null;

            using (var ms = new MemoryStream(imgData))
            {
                var srcimage = Image.FromStream(ms);
                ret = CreateFlierImageWithTearOffs(srcimage,
                                                    GetSourceUrl(flier),
                                                    Properties.Resources.PostaLogo, _qrCodeService);

                SetBlobCache("imagewithtabs", flier.Image.ToString(), flier.Id, _appStorage, ret);

                return ret;
            }    
            
        }

        public Image GetQrCodeImageForFlier(string flierId)
        {
            var flier = _queryService.FindById<PostaFlya.Domain.Flier.Flier>(flierId);
            if (flier == null || !flier.Image.HasValue)
                return null;

            var ret = GetBlobCache("qrcode", flier.Image.ToString(), flier.Id, _appStorage);
            if (ret != null)
                return ret;

            ret = CreateQrCodeImageForFlier(
                                    GetSourceUrl(flier).AddAnalyticString(FlierAnalyticSourceAction.QrCodeSrcCodeOnly),
                                    Properties.Resources.PostaLogo,
                                    _qrCodeService, Qrcodewidthheight, Logoheight);

            SetBlobCache("qrcode", flier.Image.ToString(), flier.Id, _appStorage, ret);


            return ret;
        }

        public Image GetPrintImageForFlier(string flierId, FlierPrintImageServiceQrLocation qrLocation)
        {
            var flier = _queryService.FindById<PostaFlya.Domain.Flier.Flier>(flierId);
            if (flier == null || !flier.Image.HasValue)
                return null;

            var ret = GetBlobCache(qrLocation.ToString(), flier.Image.ToString(), flier.Id, _appStorage);
            if (ret != null)
                return ret;

            var imgData = _blobStorage.GetBlob(flier.Image.Value + ImageUtil.GetIdFileExtension());
            if (imgData == null || imgData.Length == 0)
                return null;

            using (var ms = new MemoryStream(imgData))
            {
                var srcimage = Image.FromStream(ms);
                ret = CreateFlierImage(srcimage, GetSourceUrl(flier),
                                            Properties.Resources.PostaLogo, _qrCodeService, qrLocation);
                SetBlobCache(qrLocation.ToString(), flier.Image.ToString(), flier.Id, _appStorage, ret);
                
                return ret;

            }  
        }

        const int Qrcodewidthheight = 480;
        const int Logoheight = 200;

        private static Image CreateFlierImage(Image srcimage, string url, Bitmap logo, QrCodeServiceInterface qrCodeService, FlierPrintImageServiceQrLocation qrLocation)
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
            var qrimage = CreateQrCodeImageForFlier(url.AddAnalyticString(FlierAnalyticSourceAction.QrCodeSrcOnFlierWithoutTearOffs), logo, qrCodeService, qrcodewidthheight, 
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


        public static Image CreateQrCodeImageForFlier(string url
            , Image logo, QrCodeServiceInterface qrCodeService
            , int qrcodewidthheight, int logoheight)
        {
            var imgToDisp = new HashSet<IDisposable>();

            var qrimage = qrCodeService.QrCodeImg(url, qrcodewidthheight, 1);
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
                                                         , string url, Image logo, QrCodeServiceInterface qrCodeService)
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

            var qrimage = CreateQrCodeImageForFlier(url.AddAnalyticString(FlierAnalyticSourceAction.QrCodeSrcOnFlierWithTearOffs), logo, qrCodeService, qrcodewidthheight, 
                                                    logoheight);
            imgToDisp.Add(qrimage);

            var qrimagetear = qrCodeService.QrCodeImg(url.AddAnalyticString(FlierAnalyticSourceAction.QrCodeSrcTearOff), qrcodetearoffwidth, 2);
            imgToDisp.Add(qrimagetear);


            var graphics = Graphics.FromImage(target);
            imgToDisp.Add(graphics);
            graphics.FillRectangle(Brushes.White, -1, -1, target.Width + 1, target.Height + 1);
            graphics.DrawImage(resized, 0, 0);
            graphics.DrawImage(qrimage, 0, height - tearoffheight);

            var font = new Font("Times New Roman", 28, FontStyle.Regular);
            imgToDisp.Add(font);

            var blackPen = new Pen(Color.Black, 2);
            imgToDisp.Add(blackPen);

            url = url.Replace("http://", ""); //no need to print http://
            for (var i = 0; i < 10; i++)
            {
                var xOff = qrcodewidthheight + (i * tearoffwidth);
                graphics.DrawImage(qrimagetear, xOff, height - qrcodetearoffwidth);
                
                //tear off border
                graphics.DrawRectangle(blackPen, xOff, height - tearoffheight, tearoffwidth, tearoffheight);
                
                graphics.TranslateTransform(xOff + tearoffwidth, height - tearoffheight);
                graphics.RotateTransform(90);
                SizeF textSize = graphics.MeasureString(url, font);
                var scale = tearofftextheight / textSize.Width;
                graphics.ScaleTransform(scale, scale);
                var textRect = new RectangleF(0, 0, tearofftextheight / scale, tearoffwidth / scale);

                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.None };
                graphics.DrawString(url, font, Brushes.Black, textRect, fmt);

                graphics.ResetTransform();

            }

            foreach (var iDisposable in imgToDisp)
            {
                iDisposable.Dispose();
            }

            return target;
        }

        private static string GetSourceUrl(PostaFlya.Domain.Flier.Flier flier)
        {
            //return "1234567";
            if (!string.IsNullOrEmpty(flier.TinyUrl))
                return flier.TinyUrl;

            var ret = Config.Instance.GetSetting("WebsiteUrl");
            if (string.IsNullOrEmpty(ret))
                ret = "http://www.popscenes.com/";

            return ret + flier.FriendlyId;
        }

        private static Image GetBlobCache(string context, string imageId, string flyaId, BlobStorageInterface appStorage)
        {
            var key = flyaId + "/" + imageId + "/" + context;
            var bytes = appStorage.GetBlob(key);
            if (bytes == null)
                return null;

            var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        private static void SetBlobCache(string context, string imageId, string flyaId, BlobStorageInterface appStorage, Image image)
        {
            var key = flyaId + "/" + imageId + "/" + context;
            var convdata = image.GetBytes();
            appStorage.SetBlob(key, convdata, BlobProperties.JpegContentTypeDefault);
        }
    }
}