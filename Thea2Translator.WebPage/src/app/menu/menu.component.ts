import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.less']
})
export class MenuComponent implements OnInit {

  items = [
    {name:"Strona domowa", path: "/home"},
    {name:"Status tłumaczenia", path: "/translate-progress"},
    {name:"O nas", path: "/about"},
    {name:"Wsparcie", path: "/support"}
  ];

  constructor() { }

  ngOnInit() {
  }

}
