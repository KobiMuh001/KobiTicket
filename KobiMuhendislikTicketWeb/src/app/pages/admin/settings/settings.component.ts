import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, moveItemInArray, DragDropModule } from '@angular/cdk/drag-drop';
import { SystemParameterService, SystemParameterDto, CreateSystemParameterDto, UpdateSystemParameterDto } from '../../../core/services/system-parameter.service';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, DragDropModule],
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {
  groups: string[] = [];
  selectedGroup: string = '';
  items: SystemParameterDto[] = [];
  loading = false;
  // editing tracked by composite key string: `${group}:${numericKey ?? key}`
  editing: { [key: string]: boolean } = {};
  showDeleteModal = false;
  deletingItemId: number | null = null;

  // create form
  newKey = '';
  newValue = '';
  newValue2 = '';
  newDescription = '';
  // new group
  newGroupMode = false;
  newGroupName = '';
  // data types for General group
  availableDataTypes = ['String', 'Int', 'Boolean', 'Json'];
  newDataType = 'String';

  colorInputSupported = false;
  orderingChanged = false;

  constructor(private svc: SystemParameterService) {}

  ngOnInit(): void {
    try {
      const i = document.createElement('input');
      i.setAttribute('type', 'color');
      this.colorInputSupported = i.type === 'color';
    } catch {
      this.colorInputSupported = false;
    }
    this.loadGroups();
  }

  loadGroups(): void {
    this.svc.getGroups().subscribe({
      next: (res: any) => {
        this.groups = res.data || res || [];
        if (!this.selectedGroup && this.groups && this.groups.length) {
          this.selectedGroup = this.groups[0];
        }
        if (this.selectedGroup) this.loadGroup(this.selectedGroup);
      },
      error: () => {
        // fallback to default groups if server call fails
        this.groups = ['Department','TicketPriority','TicketStatus','UserRole','General'];
        if (!this.selectedGroup) this.selectedGroup = this.groups[0];
        this.loadGroup(this.selectedGroup);
      }
    });
  }

  selectGroup(g: string): void {
    this.selectedGroup = g;
    this.loadGroup(g);
  }

  loadGroup(g: string): void {
    this.loading = true;
    this.items = [];
    this.svc.getByGroup(g).subscribe({
      next: (res: any) => {
        this.items = res.data || res;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  drop(event: CdkDragDrop<SystemParameterDto[]>): void {
    moveItemInArray(this.items, event.previousIndex, event.currentIndex);
    this.orderingChanged = true;
  }

  saveOrder(): void {
    if (!this.selectedGroup) return;
    const orderedKeys = this.items.map(i => i.numericKey ?? (typeof i.key === 'number' ? i.key as number : parseInt(String(i.key || ''), 10)));
    this.svc.reorderGroup(this.selectedGroup, orderedKeys).subscribe({
      next: () => { this.orderingChanged = false; this.loadGroup(this.selectedGroup); },
      error: (err: any) => alert(err?.error?.message || 'Sıralama kaydedilemedi')
    });
  }

  startEditItem(item: SystemParameterDto): void {
    const key = `${item.group}:${item.numericKey ?? item.key}`;
    this.editing[key] = true;
  }

  cancelEditItem(item: SystemParameterDto): void {
    const key = `${item.group}:${item.numericKey ?? item.key}`;
    this.editing[key] = false;
    this.loadGroup(this.selectedGroup);
  }

  saveItem(item: SystemParameterDto): void {
    const dto: UpdateSystemParameterDto = {
      value: item.value,
      description: item.description,
      isActive: item.isActive,
      dataType: item.dataType
    };
    if (this.selectedGroup === 'TicketPriority' || this.selectedGroup === 'TicketStatus') {
      dto.value2 = item.value2;
    }
    const numericKey = item.numericKey ?? (typeof item.key === 'number' ? item.key : parseInt(String(item.key || ''), 10));
    if (!isNaN(numericKey)) {
      this.svc.updateByGroupAndKey(this.selectedGroup, numericKey, dto).subscribe({
        next: () => { const key = `${item.group}:${numericKey}`; this.editing[key] = false; },
        error: (err: any) => { alert(err?.error?.message || 'Güncelleme başarısız'); }
      });
    } else {
      alert('Güncelleme anahtarı bulunamadı');
    }
  }

  deleteItemByKey(item: SystemParameterDto): void {
    const numericKey = item.numericKey ?? (typeof item.key === 'number' ? item.key : parseInt(String(item.key || ''), 10));
    if (isNaN(numericKey)) return alert('Silme anahtarı bulunamadı');
    this.deletingItemId = null;
    this.deletingItemId = numericKey;
    // store group in deletingItemId as numericKey, group used later in confirmDelete
    this.showDeleteModal = true;
    // attach temporary data
    (this as any)._pendingDelete = { group: item.group, numericKey };
  }

  confirmDelete(): void {
    const pending = (this as any)._pendingDelete;
    if (!pending) return;
    const { group, numericKey } = pending;
    this.svc.deleteByGroupAndKey(group, numericKey).subscribe({
      next: () => {
        this.showDeleteModal = false;
        this.deletingItemId = null;
        (this as any)._pendingDelete = null;
        this.loadGroup(this.selectedGroup);
      },
      error: () => {
        this.showDeleteModal = false;
        this.deletingItemId = null;
        (this as any)._pendingDelete = null;
        alert('Silme başarısız');
      }
    });
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.deletingItemId = null;
  }

  createItem(): void {
    const parsed = parseInt(this.newKey, 10);
    const maxNumeric = this.items && this.items.length ? Math.max(...this.items.map(i => i.numericKey ?? (typeof i.key === 'number' ? i.key : parseInt(String(i.key || ''), 10) || 0))) : 0;
    const nextNumeric = maxNumeric + 1;
    const keyToSend = !isNaN(parsed) ? parsed : nextNumeric;
    // determine next sortOrder for this group (continue from max existing)
    const maxSort = this.items && this.items.length ? Math.max(...this.items.map(i => i.sortOrder ?? 0)) : 0;
    const nextSort = maxSort + 1;
    const dto: CreateSystemParameterDto = {
      group: this.selectedGroup,
      // omit `key` to let server auto-assign next numeric key for the group
      value: this.newValue.trim(),
      description: this.newDescription.trim(),
      isActive: true,
      dataType: this.newDataType,
      sortOrder: nextSort
    };
    if (this.selectedGroup === 'TicketPriority' || this.selectedGroup === 'TicketStatus') {
      dto.value2 = this.newValue2?.trim();
    }
    this.svc.create(dto).subscribe({
      next: () => {
        this.newKey = ''; this.newValue = ''; this.newValue2 = ''; this.newDescription = '';
        // reload groups and the current group items to ensure persistence
        this.loadGroups();
      },
      error: (err: any) => alert(err?.error?.message || 'Oluşturma başarısız')
    });
  }

  // Create a new group locally and select it
  addGroup(): void {
    const name = this.newGroupName?.trim();
    if (!name) {
      alert('Grup adı giriniz');
      return;
    }
    if (!this.groups.includes(name)) {
      this.groups = [name, ...this.groups];
    }
    this.newGroupName = '';
    this.newGroupMode = false;
    this.selectGroup(name);
  }

  getKey(item: SystemParameterDto): string {
    return `${item.group}:${item.numericKey ?? (typeof item.key === 'number' ? item.key : item.key ?? '')}`;
  }
}
