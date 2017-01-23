var https = require('http');

var particleServer = "217.113.232.240";
var port = 9091

exports.handler = function (event, context) {
    switch (event.header.namespace) {
        case 'Alexa.ConnectedHome.Discovery':
            handleDiscovery(event, context);
            break;

        case 'Alexa.ConnectedHome.Control':
            handleControl(event, context);
            break;

        default:
            log('Err', 'No supported namespace: ' + event.header.namespace);
            context.fail('Something went wrong');
            break;
    }
};


function handleDiscovery(accessToken, context) {
    log('dnf');
    log(JSON.stringify(context, null, 3));

    var options =
    {
        "hostname": particleServer,
        "port": port,
        "path": "/alexa/discover",
        "method": "POST",
        "headers": { "Content-Type": "application/json" }
    };

    var post_data = JSON.stringify
    (
        {
            "Test": "OK",
        }
    );

    var serverError = function (e) {
        log('Error', e.message);
        context.fail(generateControlError('Discovery', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
    };

    var callback = function (response) {
        var result = '';

        response.on('data', function (chunk) {
            result += chunk.toString('utf-8');
        });

        response.on('end', function () {
            log(result);

            context.succeed(JSON.parse(result));
        });

        response.on('error', serverError);
    };

    var req = https.request(options, callback);
    req.on('error', serverError);
    req.write(post_data);
    req.end();
}

function handleControl(event, context) {
    if (event.header.namespace === 'Alexa.ConnectedHome.Control') {
        var accessToken = event.payload.accessToken;
        var applianceId = event.payload.appliance.applianceId;
        var deviceid = event.payload.appliance.additionalApplianceDetails.deviceId;
        var message_id = event.header.messageId;
        var command = event.header.name
        var percentage
        var increment_value
        var decrement_value

        if (event.header.name == "SetPercentageRequest") {
            state = event.payload.percentageState.value;
        }
        else if (event.header.name == "IncrementPercentageRequest") {
            increment_value = event.payload.deltaPercentage.value;
        }
        else if (event.header.name == "DecrementPercentageRequest") {
            decrement_value = event.payload.deltaPercentage.value;
        }

        var options =
        {
            hostname: particleServer,
            port: port,
            path: "/alexa/invoke",
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        };

        var post_data = JSON.stringify(
            {
                "ComponentID": applianceId,
                "AccessToken": accessToken,
                "DeviceID": deviceid,
                "MessageID": message_id,
                "Command": command,
                "Percentage": percentage,
                "IncrementValue": increment_value,
                "DecrementValue": decrement_value
            }
        );

        var serverError = function (e) {
            log('Error', e.message);
            context.fail(generateControlError('TurnOnRequest', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
        };

        var callback = function (response) {
            var result = '';

            response.on('data', function (chunk) {
                result += chunk.toString('utf-8');
            });

            response.on('end', function () {
                context.succeed(result);
            });

            response.on('error', serverError);
        };

        var req = https.request(options, callback);
        req.on('error', serverError);
        req.write(post_data);
        req.end();
    }
}

/**
 * Utility functions.
 */
function log(title, msg) {
    console.log(title + ": " + msg);
}

function generateControlError(name, code, description) {
    var headers =
    {
        namespace: 'Control',
        name: name,
        payloadVersion: '1'
    };

    var payload =
    {
        exception:
        {
            code: code,
            description: description
        }
    };

    var result =
    {
        header: headers,
        payload: payload
    };

    return result;
}