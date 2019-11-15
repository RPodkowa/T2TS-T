import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-progress-bar',
  templateUrl: './progress-bar.component.html',
  styleUrls: ['./progress-bar.component.less']
})
export class ProgressBarComponent implements OnInit {

  @Input("value") value: number = 0;
  @Input("firstGradient") firstGradient;
  @Input("secondGradient") secondGradient;

  constructor() { }

  ngOnInit() {
  }

  setStyle() {
    let borderRadius = 0;

    if (this.value > 90 && this.value <= 97)
      borderRadius = 30;
    else if (this.value > 97)
      borderRadius = 50;

    let styles = {
      'background-color': this.firstGradient,
      'background-image': 'linear-gradient(to bottom right, ' + this.firstGradient + ', ' + this.secondGradient + ')',
      'height': '15px',
      'border-radius': '50px ' + borderRadius + 'px ' + borderRadius + 'px 50px',
      'width': this.value + '%'
    };
    return styles;
  }

}
