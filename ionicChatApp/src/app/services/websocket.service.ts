import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AppConfig } from '../app.config';

@Injectable({
  providedIn: 'root',
})
export class WebSocketService {
  private socket!: WebSocket;
  private usersInRoomSubject = new BehaviorSubject<string[]>([]);
  public usersInRoom$ = this.usersInRoomSubject.asObservable();
  
  private messagesSubject = new BehaviorSubject<string[]>([]);
  public messages$ = this.messagesSubject.asObservable();
  
  private connectCallCount = 0;
  public roomname!:string;

  private baseUrl = AppConfig.baseUrl;

  constructor(private http:HttpClient) {}

  connect(username: string, room: string): void {
    this.roomname=room;
    this.clearMessages();
    this.connectCallCount++;

    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      console.log('WebSocket already connected.....');
      return;
    }

    this.socket = new WebSocket(`${this.baseUrl}/ws?username=${username}&roomname=${room}`);

    this.socket.onopen = () => {
      console.log('WebSocket connection established.....');
    };

    
    this.socket.onmessage = (event) => {
      const message = event.data;
      console.log('Received WebSocket message:', message);
      if (message.startsWith('users:')) {
        const userList = message.slice(6).split(',');
        console.log('Parsed users list:', userList);
        this.usersInRoomSubject.next(userList);
      } else {
        const messages = [...this.messagesSubject.value, message];
        console.log('Parsed chat message:', messages);
        this.messagesSubject.next(messages);
      }
      this.handleIncomingMessage(message);
    };
    

    this.socket.onclose = () => {
      console.log('WebSocket connection closed');
    };

    this.socket.onerror = (error) => {
      console.error('WebSocket error:', error);
    };
  }

  private handleIncomingMessage(message: string): void {
    if (message.startsWith('Connected users in')) {
      const userList = message.replace('Connected users in ', '').split(': ')[1].split(', ');
      this.usersInRoomSubject.next(userList.filter(user => user !== 'UnknownUser'));
    } 
  }

  sendMessage(message: string): void {
    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(message);
    }
  }

  disconnect(): void {
    if (this.socket) {
      this.socket.close();
    }
  }

  clearMessages() {
    this.messagesSubject.next([]); 
  }
  
}
