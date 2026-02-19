import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SystemParameterService, SystemParameterDto, CreateSystemParameterDto, UpdateSystemParameterDto } from '../../../core/services/system-parameter.service';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {
  groups: string[] = [];
  selectedGroup: string = '';
  items: SystemParameterDto[] = [];
  loading = false;
  editing: { [id: number]: boolean } = {};
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

  startEdit(id: number): void {
    this.editing[id] = true;
  }

  cancelEdit(id: number): void {
    this.editing[id] = false;
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
    this.svc.update(item.id, dto).subscribe({
      next: () => { this.editing[item.id] = false; },
      error: (err: any) => { alert(err?.error?.message || 'Güncelleme başarısız'); }
    });
  }

  deleteItem(id: number): void {
    this.deletingItemId = id;
    this.showDeleteModal = true;
  }

  confirmDelete(): void {
    if (this.deletingItemId === null) return;
    const id = this.deletingItemId;
    this.svc.delete(id).subscribe({
      next: () => {
        this.showDeleteModal = false;
        this.deletingItemId = null;
        this.loadGroup(this.selectedGroup);
      },
      error: () => {
        this.showDeleteModal = false;
        this.deletingItemId = null;
        alert('Silme başarısız');
      }
    });
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.deletingItemId = null;
  }

  createItem(): void {
    if (!this.newKey.trim()) return alert('Key giriniz');
    // determine next sortOrder for this group (continue from max existing)
    const maxSort = this.items && this.items.length ? Math.max(...this.items.map(i => i.sortOrder ?? 0)) : 0;
    const nextSort = maxSort + 1;
    const dto: CreateSystemParameterDto = {
      group: this.selectedGroup,
      key: this.newKey.trim(),
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
}
