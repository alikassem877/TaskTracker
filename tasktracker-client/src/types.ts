export type Priority = 'Low' | 'Medium' | 'High';

export interface TaskItem {
    id: number;
    title: string;
    priority: Priority;
    isDone: boolean;
}