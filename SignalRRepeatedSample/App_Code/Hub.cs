using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


[assembly: OwinStartup(typeof(MainHub))]
public delegate void OneSeccondTickElapsedEventHandler(object sender, EventArgs e);

public sealed class SingletonHub
{
    #region Private Fields    
    static SingletonHub instance = null;
    public static int serv2ClientReqId;
    static readonly object syncRoot = new object();
    private System.Timers.Timer thOneSecondTimer;
    #endregion

    SingletonHub()
    {
    }

    #region Eventos
    public event OneSeccondTickElapsedEventHandler OneSecondTickElapsed;
    #endregion

    #region Public Methods
    public bool OneSecondTickElapsedIsMapped()
    {
        return OneSecondTickElapsed != null;
    }

    public void RaiseOneSecondTickElapsed()
    {
        OneSecondTickElapsed(this, new EventArgs());
    }

    public void OneSecondTimerFailOver()
    {
        if (thOneSecondTimer == null)
        {
            thOneSecondTimer = new System.Timers.Timer(1000);
            thOneSecondTimer.Elapsed += thOneSecond_Elapsed;
            thOneSecondTimer.Enabled = true;
        }
        else
        {
            if (!thOneSecondTimer.Enabled)
            {
                thOneSecondTimer.Enabled = true;
            }
        }
    }
    #endregion

    #region One Second Timer events
    private void thOneSecond_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (OneSecondTickElapsedIsMapped())
        {
            RaiseOneSecondTickElapsed();
        }
    }
    #endregion 

    public static SingletonHub Instance
    {
        get
        {
            lock (syncRoot)
            {
                if (instance == null)
                {
                    instance = new SingletonHub();
                }
                return instance;
            }
        }
    }
}

public class MainHub : Hub
{
    #region Owin config method
    public void Configuration(IAppBuilder app)
    {
        app.Map("/signalr", map =>
        {
            GlobalHost.Configuration.DefaultMessageBufferSize = 1;

            map.UseCors(CorsOptions.AllowAll);
            map.RunSignalR(new HubConfiguration()
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true
            });
        });
    }
    #endregion

    #region Base events
    public override System.Threading.Tasks.Task OnConnected()
    {
        mapOneSecondTickEvents();
        SingletonHub.Instance.OneSecondTimerFailOver();

        return base.OnConnected();
    }

    public override System.Threading.Tasks.Task OnReconnected()
    {
        SingletonHub.Instance.OneSecondTimerFailOver();
        return base.OnReconnected();
    }

    public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
    {
        return base.OnDisconnected(stopCalled);
    }
    #endregion 

    private void mapOneSecondTickEvents()
    {
        if (!SingletonHub.Instance.OneSecondTickElapsedIsMapped())
        {
            SingletonHub.Instance.OneSecondTickElapsed += Instance_OneSecondTickElapsed;
        }
    }

    private void Instance_OneSecondTickElapsed(object sender, EventArgs e)
    {
        SingletonHub.serv2ClientReqId++;
        GlobalHost.ConnectionManager.GetHubContext<MainHub>().Clients.All.DataAvailable(SingletonHub.serv2ClientReqId);
    }

    #region Server Methods
    public async Task<string> GetData(int reqId)
    {
        return reqId.ToString() +  " => Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquid ex ea commodi consequat. Quis aute iure reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint obcaecat cupiditat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
    }
    #endregion
}
