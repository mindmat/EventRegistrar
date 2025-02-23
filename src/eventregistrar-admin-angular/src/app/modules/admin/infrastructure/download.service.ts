import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Inject, Injectable, Optional } from '@angular/core';
import { API_BASE_URL } from 'app/api/api';

@Injectable({ providedIn: 'root' })
export class DownloadService
{
  constructor(@Inject(HttpClient) private http: HttpClient,
    @Optional() @Inject(API_BASE_URL) private baseUrl?: string) { }

  download(queryType: string, query: any, filename: string | null = null, contentType: string | null = null)
  {
    const url = this.baseUrl + `/api/${queryType}`;
    this.http.post(url, query, { observe: "response", responseType: "blob" }).subscribe((file: HttpResponse<Blob>) =>
    {
      let usedContentType = file.headers.get('content-type') ?? contentType;
      let usedFilename = filename ?? this.extractFilenameFromResponse(file.headers) ?? 'download';

      const blob = new Blob([file.body], { type: usedContentType });
      const anchor = window.document.createElement('a');
      anchor.href = window.URL.createObjectURL(blob);
      anchor.download = usedFilename;
      document.body.appendChild(anchor);
      anchor.click();
      document.body.removeChild(anchor);
      window.URL.revokeObjectURL(anchor.href);
    });
  }

  extractFilenameFromResponse(headers: HttpHeaders): string | null
  {
    try
    {
      const contentDisposition = headers.get('content-disposition');
      const filename = contentDisposition
        .split(';')[1]
        .split('filename')[1]
        .split('=')[1]
        .trim();
      return filename;
    }
    catch (error)
    {
      return headers.get('x-filename');
    }
  }
}
