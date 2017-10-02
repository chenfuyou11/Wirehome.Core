
//[TestMethod]
//public async Task TestKod()
//{
//    //http://kodi.wiki/view/JSON-RPC_API/Examples#Introspect
//    //http://kodi.wiki/view/JSON-RPC_API

//    var ip = "192.168.0.159";
//    var port = 8080;
//    var uri = $"http://{ip}:{port}/jsonrpc";
//    Dictionary<string, string> DefaultHeaders  = new Dictionary<string, string>();
//    KeyValuePair<string, string> AuthorisationHeader  = new KeyValuePair<string, string>("", "");

//   // DefaultHeaders.Add("Content-Type", "application/json-rpc");
//    AuthorisationHeader = new KeyValuePair<string, string>("", "");

//    //var jsonRpcRequest = new JsonRpcRequest
//    //{
//    //    Method = "JSONRPC.Ping",
//    //    Parameters = new object()
//    //};

//    var jsonRpcRequest = new JsonRpcRequest
//    {
//        Method = "Player.PlayPause",
//        Parameters = new { playerid = 1 }
//    };

//    //var jsonRpcRequest = new JsonRpcRequest
//    //{
//    //    Id = "1",
//    //    Method = "Player.GetActivePlayers",
//    //    Parameters = new object()
//    //};


//    var httpClientHandler = new HttpClientHandler();
//    httpClientHandler.Credentials = new NetworkCredential("kodi", "9dominik");


//    using (var httpClient = new HttpClient(httpClientHandler))
//    {
//        foreach (var header in DefaultHeaders)
//        {
//            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
//        }

//        if (!string.IsNullOrWhiteSpace(AuthorisationHeader.Key))
//        {
//            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthorisationHeader.Key, AuthorisationHeader.Value);
//        }

//        var content = new StringContent(jsonRpcRequest.ToString());
//        content.Headers.ContentType = new MediaTypeHeaderValue("application/json-rpc");

//        var response = await httpClient.PostAsync(uri, content).ConfigureAwait(false);
//        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

//        var result = JsonConvert.DeserializeObject<JsonRpcResponse<JsonPausePlayResult>>(responseBody);

//    }
//}

