import { TodoService } from './../todo.service';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Todo } from '../todo';

@Component({
  selector: 'app-todo-edit',
  templateUrl: './todo-edit.component.html',
  styleUrls: ['./todo-edit.component.css']
})
export class TodoEditComponent implements OnInit {

  todo!: Todo;

  users: string[] = [];

  constructor(private route: ActivatedRoute, private router: Router, private service: TodoService) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      let id = +params.get('id')!;
      this.service.getTodo(id).subscribe((response: Todo) => {
        this.todo = response;
      })
    });

    this.service.getUsers().subscribe((response: any) => {
      this.users = response.value.map((user: any) => user.displayName);
    });
  }

  editTodo(todo: Todo): void {
    this.todo.description = todo.description;
    this.service.editTodo(this.todo).subscribe(() => {
      this.router.navigate(['/todo-view']);
    })
  }

}
