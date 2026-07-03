using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDRSharp.GpredictConnector
{
    class TcpServer
    {
        public TcpServer(Rigctrld rigctrl)
        {
            this.rigctrl = rigctrl;  
        }

        public void Start()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, 4532);
            this.cancel_listen = new CancellationTokenSource();
            
            this.task_connectionHandler = ListenForClientConnection(cancel_listen.Token);
            Enabled?.Invoke(true);
        }

        public void Stop()
        {
            cancel_listen?.Cancel();

        }

        private async Task ListenForClientConnection(CancellationToken ct)
        {
            this.tcpListener.Start();
            ct.Register(CancelComm);

            while (!ct.IsCancellationRequested)
            {
                //blocks until a client has connected to the server
                // Use ConfigureAwait(false) to avoid marshalling to UI thread,
                // which would delay the network read and cause rigctld to time out.
                TcpClient client = await this.tcpListener.AcceptTcpClientAsync()
                    .ConfigureAwait(false);

                //only one client can connect !
                NetworkStream clientStream = client.GetStream();
                
                Connection_established();

                
                byte[] message = new byte[4096];
                int bytesRead;
                // Buffer for partial (incomplete) commands not yet terminated by \n
                string pending = string.Empty;

                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        bytesRead = 0;
                        //read message from client
                        bytesRead = await clientStream.ReadAsync(message, 0, 4096, ct).ConfigureAwait(false);
                        if (bytesRead == 0)
                        {
                            // Client disconnected
                            break;
                        }

                        var str = System.Text.Encoding.UTF8.GetString(message, 0, bytesRead);
                        pending += str;

                        // Process all complete commands (separated by \n)
                        int newlineIndex;
                        while ((newlineIndex = pending.IndexOf('\n')) >= 0)
                        {
                            // Extract one complete command (without the \n)
                            string command = pending.Substring(0, newlineIndex).TrimEnd('\r');
                            // Remove the processed command + \n from the buffer
                            pending = pending.Substring(newlineIndex + 1);

                            if (string.IsNullOrWhiteSpace(command))
                                continue; // Skip empty lines

                            var answer = rigctrl.ExecCommand(command);
                            var answerBytes = (new System.Text.ASCIIEncoding()).GetBytes(answer);
                            await clientStream.WriteAsync(answerBytes, 0, answerBytes.Length, ct).ConfigureAwait(false);
                        }
                        // Any remaining text stays in 'pending' for the next read
                    }
                    catch
                    {
                        //a socket error has occured
                        break;
                    }
                }
                client.Close();
                connection_lost();
            }
        }

        private void CancelComm()
        {
            tcpListener.Stop();
            Enabled?.Invoke(false);
        }

        
        private void Connection_established()
        {
            
            Connected?.Invoke(true);
        }

        private void connection_lost()
        {

            Connected?.Invoke(false);
        }



        public event Action<bool> Connected;
        public event Action<bool> Enabled;
        
        
        private TcpListener tcpListener;
        private Task task_connectionHandler;
        
        private CancellationTokenSource cancel_listen;
        private Rigctrld rigctrl;
    }
}
