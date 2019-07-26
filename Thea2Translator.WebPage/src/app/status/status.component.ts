import { StatusRecord } from './../_model/statusRecord';
import { StatusService } from './../_services/status.service';
import { Component, OnInit } from '@angular/core';
import { Status } from '../_model/status';

@Component({
  selector: 'app-status',
  templateUrl: './status.component.html',
  styleUrls: ['./status.component.less']
})
export class StatusComponent implements OnInit {

  status:Status;

  constructor(private statusService:StatusService) { }

  ngOnInit() {
    this.statusService.getStatus().subscribe(data =>{
      var record:Status = JSON.parse(data.toString());
      this.status = record;
    });
  }

}
