
import { useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';

import type { Priority } from '../types';

interface TaskFormProps {
  onAddTask: (title: string, priority: Priority) => void;
}

const PRIORITIES: Priority[] = ['Low', 'Medium', 'High'];
const DEFAULT_PRIORITY: Priority = 'Medium';

export function TaskForm({ onAddTask }: TaskFormProps) {
  const [title, setTitle] = useState('');
  const [priority, setPriority] = useState<Priority>(DEFAULT_PRIORITY);
  const [error, setError] = useState('');

  const handleTitleChange = (e: ChangeEvent<HTMLInputElement>) => {
    setTitle(e.target.value);
    setError('');
  };

  const handlePriorityChange = (e: ChangeEvent<HTMLSelectElement>) => {
    setPriority(e.target.value as Priority);
  };

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const trimmedTitle = title.trim();

    if (!trimmedTitle) {
      setError('Task title is required.');
      return;
    }

    onAddTask(trimmedTitle, priority);

    setTitle('');
    setPriority(DEFAULT_PRIORITY);
    setError('');
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      <div>
        <label htmlFor="task-title">Task Title</label>

        <input
          id="task-title"
          type="text"
          value={title}
          onChange={handleTitleChange}
          placeholder="Enter task title"
          aria-invalid={Boolean(error)}
          aria-describedby={error ? 'task-title-error' : undefined}
        />

        {error && (
          <p id="task-title-error" role="alert">
            {error}
          </p>
        )}
      </div>

      <div>
        <label htmlFor="task-priority">Priority</label>

        <select
          id="task-priority"
          value={priority}
          onChange={handlePriorityChange}
        >
          {PRIORITIES.map((item) => (
            <option key={item} value={item}>
              {item}
            </option>
          ))}
        </select>
      </div>

      <button type="submit">Add Task</button>
    </form>
  );
}

export default TaskForm;
