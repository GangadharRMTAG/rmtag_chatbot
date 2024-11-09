import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouteReuseStrategy } from '@angular/router';

import { IonicModule, IonicRouteStrategy } from '@ionic/angular';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { FormsModule } from '@angular/forms';
import { ChatComponent } from './chat/chat.component';
// import { WebsocketService } from './services/websocket.service';
import { WebSocketService } from './services/websocket.service';
import { HttpClientModule } from '@angular/common/http'; 
import { WelcomeComponent } from './welcome/welcome.component';

@NgModule({
  declarations: [AppComponent,ChatComponent,WelcomeComponent],
  imports: [BrowserModule, IonicModule.forRoot(), AppRoutingModule,FormsModule,HttpClientModule],
  providers: [{ provide: RouteReuseStrategy, useClass: IonicRouteStrategy },WebSocketService],
  bootstrap: [AppComponent],
})
export class AppModule {}
