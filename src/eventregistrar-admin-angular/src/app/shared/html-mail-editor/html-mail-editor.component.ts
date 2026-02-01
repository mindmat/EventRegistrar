import
{
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    ElementRef,
    EventEmitter,
    forwardRef,
    Input,
    OnDestroy,
    OnInit,
    Output,
    ViewChild
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Subject } from 'rxjs';

export interface PlaceholderItem
{
    placeholder: string;
    description: string;
}

@Component({
    selector: 'app-html-mail-editor',
    templateUrl: './html-mail-editor.component.html',
    styleUrls: ['./html-mail-editor.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => HtmlMailEditorComponent),
            multi: true
        }
    ]
})
export class HtmlMailEditorComponent implements OnInit, OnDestroy, ControlValueAccessor
{
    @ViewChild('codeEditor') codeEditor: ElementRef<HTMLTextAreaElement>;
    @ViewChild('visualEditor') visualEditor: ElementRef<HTMLDivElement>;

    @Input() placeholders: PlaceholderItem[] = [];
    @Output() htmlChange = new EventEmitter<string>();

    html: string = '';
    viewMode: 'split' | 'code' | 'preview' = 'preview';
    showPlaceholderMenu = false;
    filteredPlaceholders: PlaceholderItem[] = [];
    placeholderFilter = '';
    placeholderMenuPosition = { top: 0, left: 0 };

    private unsubscribeAll = new Subject<void>();
    private onChange: (value: string) => void = () => { };
    private onTouched: () => void = () => { };
    private isUpdatingFromCode = false;
    private isUpdatingFromVisual = false;

    constructor(
        private changeDetectorRef: ChangeDetectorRef
    ) { }

    ngOnInit(): void
    {
        this.filteredPlaceholders = this.placeholders;
    }

    ngOnDestroy(): void
    {
        this.unsubscribeAll.next();
        this.unsubscribeAll.complete();
    }

    // ControlValueAccessor implementation
    writeValue(value: string): void
    {
        this.html = value || '';
        this.updateVisualEditor();
        this.changeDetectorRef.markForCheck();
    }

    registerOnChange(fn: (value: string) => void): void
    {
        this.onChange = fn;
    }

    registerOnTouched(fn: () => void): void
    {
        this.onTouched = fn;
    }

    // Editor methods
    onHtmlInput(event: Event): void
    {
        const target = event.target as HTMLTextAreaElement;
        this.html = target.value;
        this.isUpdatingFromCode = true;
        this.updateVisualEditor();
        this.emitChange();
        this.isUpdatingFromCode = false;
    }

    onVisualInput(event: Event): void
    {
        if (this.isUpdatingFromCode)
        {
            return;
        }
        this.isUpdatingFromVisual = true;
        const target = event.target as HTMLDivElement;
        this.html = target.innerHTML;
        this.emitChange();
        this.changeDetectorRef.markForCheck();
        this.isUpdatingFromVisual = false;
    }

    onVisualKeyDown(event: KeyboardEvent): void
    {
        // Handle @ key for placeholders in visual editor
        if (event.key === '@' && this.placeholders?.length)
        {
            const selection = window.getSelection();
            if (selection && selection.rangeCount > 0)
            {
                const range = selection.getRangeAt(0);
                const rect = range.getBoundingClientRect();
                this.placeholderMenuPosition = {
                    top: rect.bottom + 5,
                    left: rect.left
                };
                setTimeout(() => this.showPlaceholders(), 10);
            }
        }

        if (this.showPlaceholderMenu && event.key === 'Escape')
        {
            this.closePlaceholderMenu();
            event.preventDefault();
        }
    }

    // Toolbar actions - apply to visual editor when in visual/split mode
    execCommand(command: string, value?: string): void
    {
        if (this.viewMode !== 'code' && this.visualEditor?.nativeElement)
        {
            this.visualEditor.nativeElement.focus();
            document.execCommand(command, false, value);
            this.syncFromVisualEditor();
        }
    }

    // Toolbar actions
    insertTag(tag: string, wrap = false): void
    {
        const textarea = this.codeEditor?.nativeElement;
        if (!textarea)
        {
            return;
        }

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const selectedText = this.html.substring(start, end);

        let insertion: string;
        let cursorOffset: number;

        if (wrap && selectedText)
        {
            insertion = `<${tag}>${selectedText}</${tag}>`;
            cursorOffset = insertion.length;
        }
        else if (wrap)
        {
            insertion = `<${tag}></${tag}>`;
            cursorOffset = tag.length + 2;
        }
        else
        {
            insertion = `<${tag}>`;
            cursorOffset = insertion.length;
        }

        this.html = this.html.substring(0, start) + insertion + this.html.substring(end);
        this.updateVisualEditor();
        this.emitChange();

        // Restore cursor position
        setTimeout(() =>
        {
            textarea.focus();
            textarea.selectionStart = textarea.selectionEnd = start + cursorOffset;
        });
    }

    insertFormatting(before: string, after: string): void
    {
        const textarea = this.codeEditor?.nativeElement;
        if (!textarea)
        {
            return;
        }

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const selectedText = this.html.substring(start, end);

        const insertion = before + selectedText + after;
        this.html = this.html.substring(0, start) + insertion + this.html.substring(end);
        this.updateVisualEditor();
        this.emitChange();

        setTimeout(() =>
        {
            textarea.focus();
            if (selectedText)
            {
                textarea.selectionStart = start;
                textarea.selectionEnd = start + insertion.length;
            }
            else
            {
                textarea.selectionStart = textarea.selectionEnd = start + before.length;
            }
        });
    }

    bold(): void
    {
        if (this.viewMode !== 'code')
        {
            this.execCommand('bold');
        }
        else
        {
            this.insertFormatting('<strong>', '</strong>');
        }
    }

    italic(): void
    {
        if (this.viewMode !== 'code')
        {
            this.execCommand('italic');
        }
        else
        {
            this.insertFormatting('<em>', '</em>');
        }
    }

    underline(): void
    {
        if (this.viewMode !== 'code')
        {
            this.execCommand('underline');
        }
        else
        {
            this.insertFormatting('<u>', '</u>');
        }
    }

    insertLink(): void
    {
        const url = prompt('Enter URL:', 'https://');
        if (!url)
        {
            return;
        }

        if (this.viewMode !== 'code' && this.visualEditor?.nativeElement)
        {
            this.visualEditor.nativeElement.focus();
            document.execCommand('createLink', false, url);
            this.syncFromVisualEditor();
            return;
        }

        const textarea = this.codeEditor?.nativeElement;
        if (!textarea)
        {
            return;
        }

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const selectedText = this.html.substring(start, end) || 'Link text';

        const linkHtml = `<a href="${url}">${selectedText}</a>`;
        this.html = this.html.substring(0, start) + linkHtml + this.html.substring(end);
        this.updateVisualEditor();
        this.emitChange();

        setTimeout(() =>
        {
            textarea.focus();
            textarea.selectionStart = textarea.selectionEnd = start + linkHtml.length;
        });
    }

    insertImage(): void
    {
        const url = prompt('Enter image URL:', 'https://');
        if (!url)
        {
            return;
        }

        const alt = prompt('Enter alt text:', 'Image');
        const imgHtml = `<img src="${url}" alt="${alt || 'Image'}" style="max-width: 100%;">`;

        if (this.viewMode !== 'code' && this.visualEditor?.nativeElement)
        {
            this.visualEditor.nativeElement.focus();
            document.execCommand('insertHTML', false, imgHtml);
            this.syncFromVisualEditor();
            return;
        }

        const textarea = this.codeEditor?.nativeElement;
        if (!textarea)
        {
            return;
        }

        const start = textarea.selectionStart;
        this.html = this.html.substring(0, start) + imgHtml + this.html.substring(start);
        this.updateVisualEditor();
        this.emitChange();

        setTimeout(() =>
        {
            textarea.focus();
            textarea.selectionStart = textarea.selectionEnd = start + imgHtml.length;
        });
    }

    insertTable(): void
    {
        const tableHtml = `
<table style="border-collapse: collapse; width: 100%;">
  <tr>
    <th style="border: 1px solid #ddd; padding: 8px; text-align: left;">Header 1</th>
    <th style="border: 1px solid #ddd; padding: 8px; text-align: left;">Header 2</th>
  </tr>
  <tr>
    <td style="border: 1px solid #ddd; padding: 8px;">Cell 1</td>
    <td style="border: 1px solid #ddd; padding: 8px;">Cell 2</td>
  </tr>
</table>`;

        if (this.viewMode !== 'code' && this.visualEditor?.nativeElement)
        {
            this.visualEditor.nativeElement.focus();
            document.execCommand('insertHTML', false, tableHtml);
            this.syncFromVisualEditor();
            return;
        }

        const textarea = this.codeEditor?.nativeElement;
        if (!textarea)
        {
            return;
        }

        const start = textarea.selectionStart;
        this.html = this.html.substring(0, start) + tableHtml + this.html.substring(start);
        this.updateVisualEditor();
        this.emitChange();

        setTimeout(() =>
        {
            textarea.focus();
            textarea.selectionStart = textarea.selectionEnd = start + tableHtml.length;
        });
    }

    insertHorizontalRule(): void
    {
        const hrHtml = '<hr style="border: 0; border-top: 1px solid #ccc; margin: 20px 0;">';

        if (this.viewMode !== 'code' && this.visualEditor?.nativeElement)
        {
            this.visualEditor.nativeElement.focus();
            document.execCommand('insertHorizontalRule');
            this.syncFromVisualEditor();
            return;
        }

        const textarea = this.codeEditor?.nativeElement;
        if (!textarea)
        {
            return;
        }

        const start = textarea.selectionStart;
        this.html = this.html.substring(0, start) + hrHtml + this.html.substring(start);
        this.updateVisualEditor();
        this.emitChange();

        setTimeout(() =>
        {
            textarea.focus();
            textarea.selectionStart = textarea.selectionEnd = start + hrHtml.length;
        });
    }

    insertParagraph(): void
    {
        if (this.viewMode !== 'code')
        {
            this.execCommand('formatBlock', 'p');
        }
        else
        {
            this.insertFormatting('<p>', '</p>');
        }
    }

    insertHeading(level: number): void
    {
        if (this.viewMode !== 'code')
        {
            this.execCommand('formatBlock', `h${level}`);
        }
        else
        {
            this.insertFormatting(`<h${level}>`, `</h${level}>`);
        }
    }

    insertList(ordered: boolean): void
    {
        if (this.viewMode !== 'code' && this.visualEditor?.nativeElement)
        {
            this.visualEditor.nativeElement.focus();
            document.execCommand(ordered ? 'insertOrderedList' : 'insertUnorderedList');
            this.syncFromVisualEditor();
            return;
        }

        const textarea = this.codeEditor?.nativeElement;
        if (!textarea)
        {
            return;
        }

        const start = textarea.selectionStart;
        const tag = ordered ? 'ol' : 'ul';
        const listHtml = `
<${tag}>
  <li>Item 1</li>
  <li>Item 2</li>
  <li>Item 3</li>
</${tag}>`;

        this.html = this.html.substring(0, start) + listHtml + this.html.substring(start);
        this.updateVisualEditor();
        this.emitChange();

        setTimeout(() =>
        {
            textarea.focus();
            textarea.selectionStart = textarea.selectionEnd = start + listHtml.length;
        });
    }

    alignText(alignment: string): void
    {
        if (this.viewMode !== 'code' && this.visualEditor?.nativeElement)
        {
            this.visualEditor.nativeElement.focus();
            document.execCommand('justify' + alignment.charAt(0).toUpperCase() + alignment.slice(1));
            this.syncFromVisualEditor();
        }
        else
        {
            this.insertFormatting(`<div style="text-align: ${alignment};">`, '</div>');
        }
    }

    // Placeholder functionality
    showPlaceholders(event?: MouseEvent): void
    {
        this.filteredPlaceholders = this.placeholders;
        this.placeholderFilter = '';
        this.showPlaceholderMenu = true;

        if (event)
        {
            this.placeholderMenuPosition = {
                top: event.clientY,
                left: event.clientX
            };
        }

        this.changeDetectorRef.markForCheck();
    }

    filterPlaceholders(): void
    {
        const filter = this.placeholderFilter.toLowerCase();
        this.filteredPlaceholders = this.placeholders.filter(
            p => p.description.toLowerCase().includes(filter) || p.placeholder.toLowerCase().includes(filter)
        );
        this.changeDetectorRef.markForCheck();
    }

    insertPlaceholder(placeholder: PlaceholderItem): void
    {
        // Insert into visual editor if active
        if (this.viewMode !== 'code' && this.visualEditor?.nativeElement)
        {
            this.visualEditor.nativeElement.focus();
            document.execCommand('insertText', false, placeholder.placeholder);
            this.syncFromVisualEditor();
            this.showPlaceholderMenu = false;
            return;
        }

        const textarea = this.codeEditor?.nativeElement;
        if (!textarea)
        {
            this.showPlaceholderMenu = false;
            return;
        }

        const start = textarea.selectionStart;
        this.html = this.html.substring(0, start) + placeholder.placeholder + this.html.substring(start);
        this.updateVisualEditor();
        this.emitChange();

        this.showPlaceholderMenu = false;

        setTimeout(() =>
        {
            textarea.focus();
            textarea.selectionStart = textarea.selectionEnd = start + placeholder.placeholder.length;
        });
    }

    closePlaceholderMenu(): void
    {
        this.showPlaceholderMenu = false;
        this.changeDetectorRef.markForCheck();
    }

    // Keyboard handling for @ trigger
    onKeyDown(event: KeyboardEvent): void
    {
        if (event.key === '@')
        {
            const textarea = this.codeEditor?.nativeElement;
            if (textarea)
            {
                const rect = textarea.getBoundingClientRect();
                this.placeholderMenuPosition = {
                    top: rect.top + 200,
                    left: rect.left + 20
                };
                setTimeout(() => this.showPlaceholders(), 10);
            }
        }

        if (this.showPlaceholderMenu && event.key === 'Escape')
        {
            this.closePlaceholderMenu();
            event.preventDefault();
        }
    }

    setViewMode(mode: 'split' | 'code' | 'preview'): void
    {
        this.viewMode = mode;
        // Sync content when switching modes
        if (mode !== 'code')
        {
            this.updateVisualEditor();
        }
        this.changeDetectorRef.markForCheck();
    }

    // Get current HTML value (for external access)
    getHtml(): string
    {
        return this.html;
    }

    private updateVisualEditor(): void
    {
        if (this.isUpdatingFromVisual)
        {
            return;
        }
        // Update visual editor content
        if (this.visualEditor?.nativeElement)
        {
            this.visualEditor.nativeElement.innerHTML = this.html;
        }
    }

    private syncFromVisualEditor(): void
    {
        if (this.visualEditor?.nativeElement)
        {
            this.html = this.visualEditor.nativeElement.innerHTML;
            this.emitChange();
            this.changeDetectorRef.markForCheck();
        }
    }

    private emitChange(): void
    {
        this.onChange(this.html);
        this.htmlChange.emit(this.html);
    }
}
