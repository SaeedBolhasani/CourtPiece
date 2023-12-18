import http from 'k6/http';
import { check, group } from 'k6';
import ws from 'k6/ws';

import { htmlReport } from "./k6-reporter.js";
import { textSummary } from "./k6-summary.js";

export function handleSummary(data) {
    return {
        "report.html": htmlReport(data),        
        stdout: textSummary(data, { indent: " ", enableColors: true }),
    };
}
export let options = {
    //vus: 10,
    //iterations: 10000,
    vus: 200,
    stages: [
        { duration: '0.3m', target: 100 }, // simulate ramp-up of traffic from 1 to 3 virtual users over 0.5 minutes.
        { duration: '0.2m', target: 400 }, // stay at 4 virtual users for 0.5 minutes
        { duration: '0.5m', target: 10 }, // ramp-down to 0 users
    ],
};
const password = "P@ssw0rd";
let a = 0;


export function setup() {

}

export default function (data) {
    let p = new player();
    p.register1();

};

class player {

    constructor() {

        let username = makeid(6);

        let response = http.post('http://localhost:5182/api/v' + __ENV.ApiVersion + '/Authentication/registeration', JSON.stringify({
            password: password,
            email: makeid(6) + '@test.com',
            username: username,
            firstname: 'saeed',
            lastname: 'bol'
        }),
            {
                headers: { 'Content-Type': 'application/json' },
            });
        check(response, { "status code should be 201": res => res.status === 201 });

        response = http.post('http://localhost:5182/api/v' + __ENV.ApiVersion + '/Authentication/login', JSON.stringify({        
            password: password,
            username: username
        }),
            {
                headers: { 'Content-Type': 'application/json' },
            });
        token = response.body;
        check(response, { "status code should be 200": res => res.status === 200 });

        var token = "Bearer " + token;
        const url = 'ws://localhost:5182/chatHub';
        const params = { headers: { "Authorization": token } };

        const res = ws.connect(url, params, function (socket) {
            socket.on('open', () => {

                socket.send('{"protocol": "json", "version": 1}\x1e');
                socket.send('{"target":"JoinToRandomRoom","type":1,"arguments":[]})\x1e');
                //console.log('connected');
            });
            socket.on('message', (data) => {
                //console.log('Message received: ', data);
                socket.close();
            });
            socket.on('close', () => console.log('disconnected'));
        });

        //console.log(res);
        check(res, { 'status is 101': (r) => r && r.status === 101 });
    }

    register1() {

    }
}
function makeid(length) {
    let result = '';
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    const charactersLength = characters.length;
    let counter = 0;
    while (counter < length) {
        result += characters.charAt(Math.floor(Math.random() * charactersLength));
        counter += 1;
    }
    return result;
}