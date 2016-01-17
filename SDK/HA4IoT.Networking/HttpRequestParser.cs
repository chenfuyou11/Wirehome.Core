﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace HA4IoT.Networking
{
    internal sealed class HttpRequestParser
    {
        private readonly HttpHeaderCollection _headers = new HttpHeaderCollection();
        private readonly List<string> _lines = new List<string>();
        private readonly string _request;
        private string _body;
        private string _httpVersion;
        private HttpMethod _method;
        private string _query;
        private string _uri;

        public HttpRequestParser(string request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            _request = request;
        }

        public bool TryParse(out HttpRequest request)
        {
            try
            {
                request = null;

                if (!TryParsePackage())
                {
                    return false;
                }

                if (!TryParseRequestHeader())
                {
                    return false;
                }

                ParseHeaders();
                ParseBody();
                ParseQuery();

                request = new HttpRequest(_method, _uri, _query, _headers, _body);
                return true;
            }
            catch (Exception)
            {
                request = null;
                return false;
            }
        }

        private void ParseHeaders()
        {
            for (var i = 0; i < _lines.Count; i++)
            {
                var isRequestLine = i == 0;
                if (isRequestLine)
                {
                    continue;
                }

                var line = _lines[i];

                var isEmptyLine = string.IsNullOrEmpty(line);
                if (isEmptyLine)
                {
                    break;
                }

                if (!line.Contains(":"))
                {
                    _headers.Add(HttpHeader.Create().WithName(line));
                }
                else
                {
                    var indexOfDelimiter = line.IndexOf(":");
                    var headerName = line.Substring(0, indexOfDelimiter).Trim();
                    var headerValue = line.Substring(indexOfDelimiter + 1).Trim();

                    _headers.Add(HttpHeader.Create().WithName(headerName).WithValue(headerValue));
                }
            }
        }

        private void ParseBody()
        {
            var bodyOffset = _request.IndexOf(Environment.NewLine + Environment.NewLine,
                StringComparison.OrdinalIgnoreCase);

            if (bodyOffset == -1)
            {
                return;
            }

            _body = _request.Substring(bodyOffset + (Environment.NewLine.Length*2));
        }

        private void ParseQuery()
        {
            if (!_uri.Contains("?"))
            {
                return;
            }

            int indexOfQuestionMark = _uri.IndexOf('?');

            _query = _uri.Substring(indexOfQuestionMark + 1);
            _uri = _uri.Substring(0, indexOfQuestionMark);
            
            // This hack is requiered to allow sending data to the server which is blocked to avoid cross site scripting.
            if (_query.StartsWith("body="))
            {
                _body = _query.Substring("body=".Length).Replace("%22", "\"").Replace("%20", " ");
                _query = null;
            }
        }

        private bool TryParsePackage()
        {
            if (string.IsNullOrEmpty(_request))
            {
                return false;
            }

            _lines.AddRange(_request.Split(new[] {Environment.NewLine}, StringSplitOptions.None));
            if (_lines.Count < 2)
            {
                return false;
            }

            return true;
        }

        private bool TryParseRequestHeader()
        {
            var requestParts = _lines.First().Split(new[] {' '}, StringSplitOptions.None);
            if (requestParts.Length != 3)
            {
                return false;
            }

            if (!Enum.TryParse(requestParts[0], true, out _method))
            {
                return false;
            }

            _uri = requestParts[1];
            _httpVersion = requestParts[2];

            return true;
        }
    }
}