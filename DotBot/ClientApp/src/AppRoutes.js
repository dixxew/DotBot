import { Char } from './components/Char';
import { Rating } from './components/Rating';

const AppRoutes = [
  {
    index: true,
    element: <Rating />
  },
  {
    path: '/char',
    element: <Char />
  }
];

export default AppRoutes;
