import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Product } from '../../../../core/services/product.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss']
})
export class ProductFormComponent {
  @Input() set product(val: Product | null) {
    if (val) {
      this.isEdit = true;
      this.form.patchValue(val);
    } else {
      this.isEdit = false;
      this.form.reset({ precio: 0 });
    }
  }
  
  @Output() save = new EventEmitter<Partial<Product>>();
  @Output() cancel = new EventEmitter<void>();

  form: FormGroup;
  isEdit = false;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      codigo: ['', Validators.required],
      nombre: ['', Validators.required],
      precio: [0, [Validators.required, Validators.min(0)]]
    });
  }

  onSubmit() {
    if (this.form.valid) {
      this.save.emit(this.form.value);
    } else {
      this.form.markAllAsTouched();
    }
  }
}
