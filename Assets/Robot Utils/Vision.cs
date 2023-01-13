using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

[RequireComponent(typeof(Camera))]
public class Vision : MonoBehaviour {
    public int imageWidth;
    public int imageHeight;
    public string listenAddress = "http://*:7663/";

    private HttpListener listener = null;
    private List<HttpListenerContext> connections = new List<HttpListenerContext>();

    const string BOUNDARY = "endofframe";

    private int frameCounter = -1;
    private int finishedFrame = -1;

    IEnumerator Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add(listenAddress);
        listener.Start();
        listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);

        var camera = GetComponent<Camera>();
        while (true)
        {
            if (connections.Count > 0) {
                var rt = RenderTexture.GetTemporary(imageWidth, imageHeight, 0, RenderTextureFormat.BGRA32);
                camera.targetTexture = rt;
                camera.enabled = true;

                yield return new WaitForEndOfFrame();

                ++frameCounter;
                var frameCount = frameCounter;
                var frameTime = Time.time;
                AsyncGPUReadback.Request(rt, 0, r => {
                    try
                    {
                        OnCompleteReadback(r, frameCount, frameTime);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                });
                RenderTexture.ReleaseTemporary(rt);
            } else {
                camera.enabled = false;
                camera.targetTexture = null;
                yield return new WaitForEndOfFrame();
            }
        }
    }

    void OnDestroy()
    {
        if (listener != null)
        {
            listener.Close();
            listener = null;
        }
    }

    void OnApplicationQuit()
    {
        AsyncGPUReadback.WaitAllRequests();
    }

    void ListenerCallback(IAsyncResult result)
    {
        HttpListener listener = (HttpListener) result.AsyncState;
        // Call EndGetContext to complete the asynchronous operation.
        HttpListenerContext context = listener.EndGetContext(result);

        string widthStr = context.Request.QueryString["width"];
        string heightStr = context.Request.QueryString["height"];
        if (widthStr != null && heightStr != null) {
            try {
                int width = Int32.Parse(widthStr);
                int height = Int32.Parse(heightStr);
                // assign new values if strings parsed correctly
                imageWidth = width;
                imageHeight = height;
            } catch (FormatException ex) {
                Debug.LogException(ex);
            }
        }

        context.Response.ContentEncoding = Encoding.UTF8;
        context.Response.ContentType = "multipart/x-mixed-replace; boundary=" + BOUNDARY;
        context.Response.SendChunked = true;
        WriteBoundary(context.Response.OutputStream);
        lock (connections)
        {
            connections.Add(context);
        }

        listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
    }

    void OnCompleteReadback(AsyncGPUReadbackRequest request, int frameCount, double frameTime)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }
        var pixelData = request.GetData<uint>().ToArray();
        var width = request.width;
        var height = request.height;
        System.Threading.ThreadPool.QueueUserWorkItem(o => {
            try
            {
                var imgData = ImageConversion.EncodeArrayToJPG(pixelData, GraphicsFormat.B8G8R8A8_SRGB, (uint)width, (uint)height, 0);
                //File.WriteAllBytes(string.Format("test_{0}.jpg", frameCount), imgData);
                lock (connections)
                {
                    if (finishedFrame < frameCount)
                    {
                        finishedFrame = frameCount;
                        for (int i = connections.Count - 1; i >= 0; i--)
                        {
                            var context = connections[i];
                            try
                            {
                                Write(context.Response.OutputStream, "Content-Type: image/jpeg\r\n");
                                Write(context.Response.OutputStream, string.Format("Content-Length: {0}\r\n", imgData.Length));
                                Write(context.Response.OutputStream, string.Format("X-Frame-Time: {0}\r\n", frameTime));
                                Write(context.Response.OutputStream, "\r\n");
                                Write(context.Response.OutputStream, imgData);
                                WriteBoundary(context.Response.OutputStream);
                            }
                            catch (IOException)
                            {
                                connections.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        });
    }

    static void Write(Stream stream, byte[] data)
    {
        stream.Write(data, 0, data.Length);
    }
    static void Write(Stream stream, string data)
    {
        Write(stream, Encoding.UTF8.GetBytes(data));
    }
    static void WriteBoundary(Stream stream)
    {
        Write(stream, string.Format("\r\n--{0}\r\n", BOUNDARY));
        stream.Flush();
    }
}