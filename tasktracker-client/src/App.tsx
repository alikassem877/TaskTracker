
import { useState } from 'react';

import TaskForm from './components/TaskForm';
import TaskList from './components/TaskList';

import type { TaskItem, Priority } from './types';

type TaskFilter = 'All' | 'Active' | 'Done';

const FILTERS: TaskFilter[] = ['All', 'Active', 'Done'];

function App() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [filter, setFilter] = useState<TaskFilter>('All');

  const addTask = (title: string, priority: Priority) => {
    const newTask: TaskItem = {
      id: Date.now(),
      title,
      priority,
      isDone: false,
    };

    setTasks(previous => [...previous, newTask]);
  };

  const toggleTask = (id: number) => {
    setTasks(previous =>
      previous.map(task =>
        task.id === id
          ? { ...task, isDone: !task.isDone }
          : task
      )
    );
  };

  const deleteTask = (id: number) => {
    setTasks(previous =>
      previous.filter(task => task.id !== id)
    );
  };

  const filteredTasks = tasks.filter(task => {
    if (filter === 'Active') return !task.isDone;
    if (filter === 'Done') return task.isDone;
    return true;
  });

  const completedCount = tasks.filter(task => task.isDone).length;

  return (
    <div>
      <h1>Task Tracker</h1>

      <TaskForm onAddTask={addTask} />

      <div>
        {FILTERS.map(option => (
          <button
            key={option}
            onClick={() => setFilter(option)}
          >
            {option}
          </button>
        ))}
      </div>

      <p>
        {completedCount} of {tasks.length} done
      </p>

      <TaskList
        tasks={filteredTasks}
        onToggle={toggleTask}
        onDelete={deleteTask}
      />
    </div>
  );
}

export default App;
