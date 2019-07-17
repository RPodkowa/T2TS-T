import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserModule } from '@angular/platform-browser';
import { MenuComponent } from './menu/menu.component';
import { ProjectComponent } from './project/project.component';
import { StatusComponent } from './status/status.component';
import { TranslationComponent } from './translation/translation.component';
import { SupportComponent } from './support/support.component';
import { ContactComponent } from './contact/contact.component';
import { StatusService } from './_services/status.service';
import { HttpClientModule } from '@angular/common/http';
import { ProgressBarComponent } from './progress-bar/progress-bar.component'; 

@NgModule({
  declarations: [
    AppComponent,
    MenuComponent,
    ProjectComponent,
    StatusComponent,
    TranslationComponent,
    SupportComponent,
    ContactComponent,
    ProgressBarComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgbModule 
  ],
  providers: [StatusService],
  bootstrap: [AppComponent]
})
export class AppModule { }
