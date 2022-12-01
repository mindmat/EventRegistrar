import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { HttpClient, HttpErrorResponse, HttpEventType } from '@angular/common/http';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, Input, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { API_BASE_URL } from 'app/api/api';
import { catchError, delay, tap, throwError } from 'rxjs';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FileUploadComponent
{
  @Input()
  set fileTypes(fileTypes: string)
  {
    this._fileTypes = fileTypes;
    this.info = fileTypes;
  }
  @Input() uploadUrl: string;

  isDragging: boolean;
  progress: number;
  private _fileTypes: string;
  state: 'ready' | 'uploading' | 'success' | 'failed' = 'ready';
  lastError?: string = null;
  info?: string = null;
  file: File;

  constructor(@Inject(HttpClient) private http: HttpClient,
    private changeDetector: ChangeDetectorRef,
    private translationService: TranslateService,
    @Inject(API_BASE_URL) private baseUrl?: string) { }

  fileSelected(ev: Event)
  {
    const element = ev.currentTarget as HTMLInputElement;
    let fileList: FileList | null = element.files;
    if (fileList)
    {
      this.uploadFile(fileList[0]);
    }
  }

  onDragEnter(ev: Event)
  {
    this.isDragging = true;
    ev.preventDefault();
    ev.stopPropagation();
  }

  onDragOver(ev: Event)
  {
    this.isDragging = true;
    ev.preventDefault();
    ev.stopPropagation();
  }

  onFileDropped(event: DragEvent)
  {
    this.isDragging = false;
    event.preventDefault();
    event.stopPropagation();

    this.uploadFile(event.dataTransfer.files[0]);
  }

  uploadFile(file: File)
  {
    this.file = file;
    this.lastError = null;

    this.state = 'uploading';
    this.info = this.translationService.instant('Uploading', { filename: file.name });
    this.changeDetector.detectChanges();

    const url = `${this.baseUrl}/${this.uploadUrl}`;
    let formData: FormData = new FormData();
    formData.append('file', file);

    this.http.post(url, formData)
      .pipe(
        catchError((err: HttpErrorResponse) =>
        {
          this.lastError = err.error;
          this.state = 'failed';
          this.info = null;
          this.changeDetector.detectChanges();
          return throwError(() => new Error('Something bad happened; please try again later.'));
        }),
        tap(_ =>
        {
          this.state = 'success';
          this.info = this.translationService.instant('UploadSuccessful');
          this.changeDetector.detectChanges();
        }),
        delay(5000),
        tap(_ =>
        {
          this.state = 'ready';
          this.info = this._fileTypes;
          this.changeDetector.detectChanges();
        })
      ).subscribe();
  }
}
