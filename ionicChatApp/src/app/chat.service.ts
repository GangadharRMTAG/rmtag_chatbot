import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private socket!: WebSocket;
  private messages: Subject<string>;

  constructor() {
    this.messages = new Subject<string>();
  }

  public connect(url: string): void {
    this.socket = new WebSocket(url);

    this.socket.onmessage = (event) => {
      this.messages.next(event.data);
    };
  }

  public sendMessage(message: string): void {
    this.socket.send(message);
  }

  public getMessages(): Subject<string> {
    return this.messages;
  }
}
