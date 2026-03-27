export enum ProjectStatus {
  Draft = 0,
  GeneratingScript = 1,
  UserReview = 2,
  GeneratingAudio = 3,
  GeneratingVideo = 4,
  Completed = 5
}

export interface Project {
  id: string;
  title: string;
  description?: string;
  status: ProjectStatus;
  generatedScript?: string;
  audioUrl?: string;
  videoUrl?: string;
  _cached?: boolean; // temporary flag for UI
}

export interface User {
  id: string;
  email: string;
}
