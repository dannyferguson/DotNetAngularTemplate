import {inject, Injectable} from '@angular/core';
import {MatSnackBar} from '@angular/material/snack-bar';
import {SnackBar} from './snackbar.component';

type AlertBannerType = 'error' | 'warning' | 'info' | 'success';

@Injectable({
  providedIn: 'root'
})
export class SnackBarService {
  private _snackBar = inject(MatSnackBar);

  public fire(type: AlertBannerType, message: string, durationInMs: number = 86400000): void {
    this._snackBar.openFromComponent(SnackBar, {
      duration: durationInMs,
      data: message,
      panelClass: type + '-snackbar',
    });
  }
}
