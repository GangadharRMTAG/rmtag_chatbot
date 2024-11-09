import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { WebSocketService } from '../services/websocket.service'; 

interface JoinRoomResponse {
  message: string;
  navigate: boolean;
}
@Component({
  selector: 'app-home',
  templateUrl: './home.page.html',
  styleUrls: ['./home.page.scss'],
})
export class HomePage implements OnInit {
  joinRoomForm!: FormGroup;
  fb = inject(FormBuilder);
  private websocketService: WebSocketService = inject(WebSocketService);

  constructor(private route: Router, private http: HttpClient) { }

  ngOnInit(): void {
    this.joinRoomForm = this.fb.group({
      user: ['', Validators.required],
      room: ['', Validators.required]
    });
  }

  joinRoom() {
    if (this.joinRoomForm.valid) {
      const { user, room } = this.joinRoomForm.value;
      console.log('Joining room with', { user, room });

      // this.http.post<JoinRoomResponse>('http://192.168.1.9:8080/api/user/join', {
      this.http.post<JoinRoomResponse>('https://rmtagchatbot-production.up.railway.app/api/user/join', {
        Username: user,
        Roomname: room
      }).subscribe((response: JoinRoomResponse) => {
        if (response.navigate) {
          this.route.navigate(['/chat', { roomname: room }]);
          this.websocketService.connect(user, room);
        }
      }, error => {
        console.error('Error adding user:', error);
      });
    }
  }
  
}
