import { Component, OnInit } from '@angular/core';
import { WebSocketService } from '../services/websocket.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { UserService } from '../services/user.service';

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
  roomName: string = '';
  rn:string='';

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
    this.rn = this.webSocketService.roomname;
    // console.log('--',this.rn);
    this.fetchAllUsers(this.rn); 
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
    console.log('disconnect () called...');
    this.webSocketService.disconnect();
  }

  leaveChat() {
    console.log('leave chat method');
    this.disconnect();
    this.webSocketService.clearMessages();
    this.username='';
    this.roomname='';
    this.message='';
    this.messages=[];
    this.usersInRoom=[];
    this.router.navigate(['/home']);
  }

  //  private baseUrl = 'http://192.168.1.11:8080';
   private baseUrl = 'https://rmtagchatbot-production.up.railway.app';
  allUsersInRoom: string[] = [];
  fetchAllUsers(roomName: string): void {
    this.http.get<string[]>(`${this.baseUrl}/api/user/GetAllUsersInRoom/${this.rn}`).subscribe(
      (users) => {
        console.log('Fetched users:', users);
        this.allUsersInRoom = users;
      },
      (error) => {
        console.error('Failed to fetch all users in room', error);
      }
    );
  }  
}
