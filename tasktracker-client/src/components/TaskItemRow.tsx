
import type { TaskItem } from '../types';

interface TaskItemRowProps {
  task: TaskItem;
  onToggle: (id: number) => void;
  onDelete: (id: number) => void;
}

function TaskItemRow({ task, onToggle, onDelete }: TaskItemRowProps) {
  return (
    <li>
      <span
        style={{
          textDecoration: task.isDone ? 'line-through' : 'none',
        }}
      >
        {task.title} — {task.priority}
      </span>

      <button onClick={() => onToggle(task.id)}>
        {task.isDone ? 'Undo' : 'Complete'}
      </button>

      <button onClick={() => onDelete(task.id)}>
        Delete
      </button>
    </li>
  );
}

export default TaskItemRow;
