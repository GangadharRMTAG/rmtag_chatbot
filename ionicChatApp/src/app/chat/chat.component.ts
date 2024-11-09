import { Component } from '@angular/core';
import { WebSocketService } from '../services/websocket.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})

export class ChatComponent {
  username: string = '';
  roomname: string = '';
  message: string = '';
  messages: string[] = [];
  usersInRoom: string[] = [];

  constructor(private webSocketService: WebSocketService, private router: Router, private route: ActivatedRoute,
    private http: HttpClient
  ) {
    this.webSocketService.messages$.subscribe((messages) => {
      this.messages = messages; 
    });

    this.webSocketService.usersInRoom$.subscribe((users) => {
      console.log('Updated usersInRoom:', users);
      this.usersInRoom = users;
    });   
  } 

  connect() {
    this.webSocketService.connect(this.username, this.roomname);
  }

  sendMessage() {
    if (this.message.trim()) {
      this.webSocketService.sendMessage(this.message);
      this.message = '';
    }
  }

  disconnect() {
    this.webSocketService.disconnect();
  }

  leaveChat() {
    console.log('leave chat method');
    this.router.navigate(['/home']);
  }
  
}


