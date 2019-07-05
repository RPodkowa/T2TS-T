import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TranslateProgressComponent } from './translate-progress.component';

describe('TranslateProgressComponent', () => {
  let component: TranslateProgressComponent;
  let fixture: ComponentFixture<TranslateProgressComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TranslateProgressComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TranslateProgressComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
