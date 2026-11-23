
import type { TaskItem } from '../types';
import TaskItemRow from './TaskItemRow';

interface TaskListProps {
  tasks: TaskItem[];
  onToggle: (id: number) => void;
  onDelete: (id: number) => void;
}

function TaskList({ tasks, onToggle, onDelete }: TaskListProps) {
  if (tasks.length === 0) {
    return <p>No tasks found.</p>;
  }

  return (
    <ul>
      {tasks.map(task => (
        <TaskItemRow
          key={task.id}
          task={task}
          onToggle={onToggle}
          onDelete={onDelete}
        />
      ))}
    </ul>
  );
}

export default TaskList;
