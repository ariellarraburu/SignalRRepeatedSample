'use strict';
var mainHub;
var signalrStarted;

$(document).ready(function () {
    initSignalr();
});

function initSignalr() {
    
    signalrStarted = false;
    mainHub = $.connection.mainHub;

    if (!mainHub.client.dataAvailable) {
        mainHub.client.dataAvailable = function () {

            var pCallback = $.connection.hub.proxies.mainhub._.callbackMap;
            if (pCallback.dataavailable.length) {
                console.log('handlers count: ' + pCallback.dataavailable[0].eventHandlers.length.toString());
            }

            mainHub.server.getData()
                .done(function (data) {
                    //console.log(data);
                })
                .fail(function (error) {
                    console.log('Error calling getData: ' + error);
                });
        };
    }

    hubClientStart();

    $.connection.hub.connectionSlow(function () {
        console.log('Alerta de conexión lenta');
    });

    $.connection.hub.disconnected(function () {
        setTimeout(function () {
            hubClientStart();
        }, 5000); 
        if ($.connection.hub.lastError) { console.log('Disconnected. Reason: ' + $.connection.hub.lastError.message); }
    });
}

function hubClientStart() {
    $.connection.hub.logging = false;
    $.connection.hub.start().done(function () {
        signalrStarted = true;        
    }).fail(function () {
        console.log('Error on connect')
        setTimeout(function () {
            hubClientStart();
        }, 5000); 
    });
}