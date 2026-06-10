export interface Application {
  id: string;
  userId?: string | null;
  company: string;
  role: string;
  type: string;
  status: string;
  dateApplied: string | null;
  link: string | null;
  notes: string | null;
  createdAt: string;
}

export const STATUS_OPTIONS: string[] = [
  'Wishlist',
  'Applied',
  'Interview',
  'Offer',
  'Rejected',
];

export const TYPE_OPTIONS: string[] = [
  'Internship',
  'Job',
];
