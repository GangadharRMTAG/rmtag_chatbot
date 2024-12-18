import { Component } from '@angular/core';
import { WebSocketService } from '../services/websocket.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AppConfig } from '../app.config';

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

  private baseUrl = AppConfig.baseUrl;

  allUsersInRoom: string[] = [];
  fetchAllUsers(roomName: string): void {
    console.log('Fetching all users in room:', roomName);
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

  doRefresh(event: any): void {
    console.log('Begin doRefresh()...');
        this.fetchAllUsers('this.rn'); 

    setTimeout(() => {
      console.log('end doRefresh()...');
      event.target.complete(); 
    }, 2000); 
  }
  
  // using polling ---
  // private pollingInterval : any;
  // ngOnInit(): void {
  //   this.pollingInterval = setInterval(() => {
  //     this.fetchAllUsers(this.rn); 
  //   }, 10000);
  // }
  
  // ngOnDestroy(): void {
  //   if (this.pollingInterval) {
  //     clearInterval(this.pollingInterval);
  //   }
  // }
}

