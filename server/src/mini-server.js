import { v4 as uuidV4 } from "uuid";
import crypto from "crypto";;
import express from "express";
import { createServer } from "http";
import { WebSocketServer } from 'ws';

const app = express();
const port = 40283;

const server = createServer(app);
const wss = new WebSocketServer({ server });

const sessions = {};
class Session {
    sceneState = {};
    constructor(id) {
        this.id = id;
        this.clients = {};
        sessions[id] = this;
    }
    addClient(userId, ws) {
        this.clients[userId] = ws;
        ws.on('message', (data) => {
            const msgData = JSON.parse(data.toString());
            this.evaluateMessage(msgData);
        });
        this.send(userId, { type: 'connected', userId: userId, sceneState: this.sceneState });
    }
    evaluateMessage(data) {
        switch (data.type) {
            case 'rpc':
                this.rpc(data);
                break;
            case 'rpc_set':
                this.rpc_set(data);
                break;
            default:
                break;
        }
    }
    send(userId, data) {
        this.clients[userId].send(JSON.stringify(data));
    }
    rpc_set(data) {
        // data["userId"] = userId;
        // data["type"] = "rpc_set";
        // data["objectId"] = objectId;
        // data["key"] = propertyName;
        // data["value"] = value;
        // data["component"] = component;
        //set the value in the sceneState
        if (!this.sceneState[data.objectId]) {
            this.sceneState[data.objectId] = {};
        }
        if (!this.sceneState[data.objectId][data.component]) {
            this.sceneState[data.objectId][data.component] = {};
        }
        this.sceneState[data.objectId][data.component][data.key] = data.value;
        //send to all other clients
        for (const userId in this.clients) {
            if (userId != data.userId) {
                this.send(userId, data);
            } else {
                this.send(userId, { received: true });
            }
        }
    }
    rpc(data) {
        for (const userId in this.clients) {
            if (userId != data.userId) {
                this.send(userId, data);
            } else {
                this.send(userId, { received: true });
            }
        }
    }

}

wss.on('connection', function (ws) {
    console.log("client joined.")
    ws.on('message', function (data) {
        const msgData = JSON.parse(data.toString());
        if (msgData.type == 'connect') {
            console.log("connected")
            if (!sessions[msgData.sessionId]) {
                sessions[msgData.sessionId] = new Session(msgData.sessionId);
            }
            sessions[msgData.sessionId].addClient(msgData.userId, ws);
        }

    });
    ws.on('close', function () {
        console.log("client left.");
    });
});

server.listen(port, function () {
    console.log(`Listening on http://localhost:${port}`);
});