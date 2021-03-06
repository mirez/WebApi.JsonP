﻿// copyright for this file: Microsoft
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace WebApi.JsonP
{
	public static class UriFormatExtensions
    {
        public static void SetUriExtensionMappings(this HttpApplication application, IEnumerable<UriExtensionMapping> mappings)
        {
            UriFormatExtensionMessageChannel.SetUriExtensionMappings(mappings);
        }
    }
    
    public class UriFormatExtensionMessageChannel : DelegatingChannel
    {
        public UriFormatExtensionMessageChannel(HttpMessageChannel handler)
            :base(handler)
        {
        }

        private static Dictionary<string, MediaTypeWithQualityHeaderValue> extensionMappings = new Dictionary<string, MediaTypeWithQualityHeaderValue>();

        public static void SetUriExtensionMappings(IEnumerable<UriExtensionMapping> mappings)
        {
            foreach(var mapping in mappings)
            {
                extensionMappings[mapping.Extension] = mapping.MediaType;
            }
        }

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var segments = request.RequestUri.Segments;
            var lastSegment = segments.LastOrDefault();
            MediaTypeWithQualityHeaderValue mediaType;
            var found = extensionMappings.TryGetValue(lastSegment, out mediaType);
            
            if (found)
            {
                var newUri = request.RequestUri.OriginalString.Replace("/" + lastSegment, "");
                request.RequestUri = new Uri(newUri, UriKind.Absolute);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(mediaType);
            }
            return base.SendAsync(request, cancellationToken);
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class UriExtensionMapping
    {
        public string Extension { get; set; }

        public MediaTypeWithQualityHeaderValue MediaType { get; set; }

    }

    public static class UriExtensionMappingExtensions
    {
        public static void AddMapping(this IList<UriExtensionMapping> mappings, string extension, string mediaType)
        {
            mappings.Add(new UriExtensionMapping { Extension = extension, MediaType = new MediaTypeWithQualityHeaderValue(mediaType) });
        }
    }

}