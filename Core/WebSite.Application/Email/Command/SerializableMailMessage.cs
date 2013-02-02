using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Net.Mime;
using System.Collections.Specialized;

namespace Website.Application.Email.Command
{

    [Serializable]
    public class SerializableMailMessage
    {
        internal Boolean IsBodyHtml { get; set; }
        internal String Body { get; set; }
        internal SerializeableMailAddress From { get; set; }
        internal List<SerializeableMailAddress> To { get; set; }
        internal List<SerializeableMailAddress> Cc { get; set; }
        internal List<SerializeableMailAddress> Bcc { get; set; }
        internal List<SerializeableMailAddress> ReplyTo { get; set; }

        internal SerializeableMailAddress Sender { get; set; }
        internal String Subject { get; set; }
        internal List<SerializeableAttachment> Attachments { get; set; }
        internal Encoding BodyEncoding { get; set; }
        internal Encoding SubjectEncoding { get; set; }
        internal DeliveryNotificationOptions DeliveryNotificationOptions { get; set; }
        internal SerializeableCollection Headers { get; set; }
        internal MailPriority Priority { get; set; }
        internal List<SerializeableAlternateView> AlternateViews { get; set; }

        public SerializableMailMessage()
        {
            To = new List<SerializeableMailAddress>();
            Cc = new List<SerializeableMailAddress>();
            Bcc = new List<SerializeableMailAddress>();
            ReplyTo = new List<SerializeableMailAddress>();
            Attachments = new List<SerializeableAttachment>();
            AlternateViews = new List<SerializeableAlternateView>();
        }

    }

    [Serializable]
    internal class SerializeableLinkedResource
    {
        private String _contentId;
        private Uri _contentLink;
        private Stream _contentStream;
        private SerializeableContentTypeMime _contentTypeMime;
        private TransferEncoding _transferEncoding;

        internal static SerializeableLinkedResource GetSerializeableLinkedResource(LinkedResource lr)
        {
            if (lr == null)
                return null;

            var slr = new SerializeableLinkedResource {_contentId = lr.ContentId, _contentLink = lr.ContentLink};

            if (lr.ContentStream != null)
            {
                var bytes = new byte[lr.ContentStream.Length];
                lr.ContentStream.Read(bytes, 0, bytes.Length);
                slr._contentStream = new MemoryStream(bytes);
            }

            slr._contentTypeMime = SerializeableContentTypeMime.GetSerializeableContentTypeMime(lr.ContentType);
            slr._transferEncoding = lr.TransferEncoding;

            return slr;

        }

        internal LinkedResource GetLinkedResource()
        {
            var slr = new LinkedResource(_contentStream)
                {
                    ContentId = _contentId,
                    ContentLink = _contentLink,
                    ContentType = _contentTypeMime.GetContentType(),
                    TransferEncoding = _transferEncoding
                };

            return slr;
        }
    }

    [Serializable]
    internal class SerializeableAlternateView
    {

        private Uri _baseUri;
        private String _contentId;
        private Stream _contentStream;
        private SerializeableContentTypeMime _contentTypeMime;
        private readonly List<SerializeableLinkedResource> _linkedResources = new List<SerializeableLinkedResource>();
        private TransferEncoding _transferEncoding;

        internal static SerializeableAlternateView GetSerializeableAlternateView(AlternateView av)
        {
            if (av == null)
                return null;

            var sav = new SerializeableAlternateView {_baseUri = av.BaseUri, _contentId = av.ContentId};

            if (av.ContentStream != null)
            {
                var bytes = new byte[av.ContentStream.Length];
                av.ContentStream.Read(bytes, 0, bytes.Length);
                sav._contentStream = new MemoryStream(bytes);
            }

            sav._contentTypeMime = SerializeableContentTypeMime.GetSerializeableContentTypeMime(av.ContentType);

            foreach (LinkedResource lr in av.LinkedResources)
                sav._linkedResources.Add(SerializeableLinkedResource.GetSerializeableLinkedResource(lr));

            sav._transferEncoding = av.TransferEncoding;
            return sav;
        }

        internal AlternateView GetAlternateView()
        {

            var sav = new AlternateView(_contentStream)
                {BaseUri = _baseUri, ContentId = _contentId, ContentType = _contentTypeMime.GetContentType()};

            foreach (SerializeableLinkedResource lr in _linkedResources)
                sav.LinkedResources.Add(lr.GetLinkedResource());

            sav.TransferEncoding = _transferEncoding;
            return sav;
        }
    }

    [Serializable]
    internal class SerializeableMailAddress
    {
        private String _address;
        private String _displayName;

        internal static SerializeableMailAddress GetSerializeableMailAddress(MailAddress ma)
        {
            if (ma == null)
                return null;
            var sma = new SerializeableMailAddress {_address = ma.Address, _displayName = ma.DisplayName};

            return sma;
        }

        internal MailAddress GetMailAddress()
        {
            var ret = new MailAddress(_address, _displayName);
            return ret;
        }
    }

    [Serializable]
    internal class SerializeableContentDisposition
    {
        private DateTime _creationDate;
        private String _disposition;
        private String _fileName;
        private Boolean _inline;
        private DateTime _modificationDate;
        private SerializeableCollection _parameters;
        private DateTime _readDate;
        private long _size;

        internal static SerializeableContentDisposition GetSerializeableContentDisposition(ContentDisposition cd)
        {
            if (cd == null)
                return null;

            var scd = new SerializeableContentDisposition
                {
                    _creationDate = cd.CreationDate,
                    _disposition = cd.DispositionType,
                    _fileName = cd.FileName,
                    _inline = cd.Inline,
                    _modificationDate = cd.ModificationDate,
                    _parameters = SerializeableCollection.GetSerializeableCollection(cd.Parameters),
                    _readDate = cd.ReadDate,
                    _size = cd.Size
                };

            return scd;
        }

        internal void SetContentDisposition(ContentDisposition scd)
        {
            scd.CreationDate = _creationDate;
            scd.DispositionType = _disposition;
            scd.FileName = _fileName;
            scd.Inline = _inline;
            scd.ModificationDate = _modificationDate;
            _parameters.SetColletion(scd.Parameters);

            scd.ReadDate = _readDate;
            scd.Size = _size;
        }
    }

    [Serializable]
    internal class SerializeableContentTypeMime
    {
        private String _boundary;
        private String _charSet;
        private String _media;
        private String _name;
        private SerializeableCollection _parameters;

        internal static SerializeableContentTypeMime GetSerializeableContentTypeMime(ContentType ct)
        {
            if (ct == null)
                return null;

            var sct = new SerializeableContentTypeMime
                {
                    _boundary = ct.Boundary,
                    _charSet = ct.CharSet,
                    _media = ct.MediaType,
                    _name = ct.Name,
                    _parameters = SerializeableCollection.GetSerializeableCollection(ct.Parameters)
                };

            return sct;
        }

        internal ContentType GetContentType()
        {

            var sct = new ContentType {Boundary = _boundary, CharSet = _charSet, MediaType = _media, Name = _name};

            _parameters.SetColletion(sct.Parameters);

            return sct;
        }
    }

    [Serializable]
    internal class SerializeableAttachment
    {
        private String _contentId;
        private SerializeableContentDisposition _contentDisposition;
        private SerializeableContentTypeMime _contentTypeMime;
        private Stream _contentStream;
        private TransferEncoding _transferEncoding;
        private String _name;
        private Encoding _nameEncoding;

        internal static SerializeableAttachment GetSerializeableAttachment(Attachment att)
        {
            if (att == null)
                return null;

            var saa = new SerializeableAttachment
                {
                    _contentId = att.ContentId,
                    _contentDisposition =
                        SerializeableContentDisposition.GetSerializeableContentDisposition(att.ContentDisposition)
                };

            if (att.ContentStream != null)
            {
                var bytes = new byte[att.ContentStream.Length];
                att.ContentStream.Read(bytes, 0, bytes.Length);

                saa._contentStream = new MemoryStream(bytes);
            }

            saa._contentTypeMime = SerializeableContentTypeMime.GetSerializeableContentTypeMime(att.ContentType);
            saa._name = att.Name;
            saa._transferEncoding = att.TransferEncoding;
            saa._nameEncoding = att.NameEncoding;
            return saa;
        }

        internal Attachment GetAttachment()
        {
            var saa = new Attachment(_contentStream, _name) {ContentId = _contentId};
            _contentDisposition.SetContentDisposition(saa.ContentDisposition);

            saa.ContentType = _contentTypeMime.GetContentType();
            saa.Name = _name;
            saa.TransferEncoding = _transferEncoding;
            saa.NameEncoding = _nameEncoding;
            return saa;
        }
    }

    [Serializable]
    internal class SerializeableCollection
    {
        readonly Dictionary<String, String> _collection = new Dictionary<String, String>();

        internal static SerializeableCollection GetSerializeableCollection(NameValueCollection col)
        {
            if (col == null)
                return null;

            var scol = new SerializeableCollection();
            foreach (String key in col.Keys)
                scol._collection.Add(key, col[key]);

            return scol;
        }

        internal static SerializeableCollection GetSerializeableCollection(StringDictionary col)
        {
            if (col == null)
                return null;

            var scol = new SerializeableCollection();
            foreach (String key in col.Keys)
                scol._collection.Add(key, col[key]);

            return scol;
        }

        internal void SetColletion(NameValueCollection scol)
        {

            foreach (var key in _collection.Keys)
            {
                scol.Add(key, _collection[key]);
            }

        }

        internal void SetColletion(StringDictionary scol)
        {

            foreach (var key in _collection.Keys)
            {
                if (scol.ContainsKey(key))
                    scol[key] = _collection[key];
                else
                    scol.Add(key, _collection[key]);
            }
        }
    }


}