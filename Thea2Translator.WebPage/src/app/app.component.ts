import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {
  title = 'Thea 2 PL';

  scroll(elemName: HTMLElement) {
    elemName.scrollIntoView();
  }
}
