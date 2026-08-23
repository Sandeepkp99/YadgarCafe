import "./Button.css";
import { ButtonHTMLAttributes } from "react";

type Props = ButtonHTMLAttributes<HTMLButtonElement> & {
  title: string;
}

export default function Button({ title, ...props }: Props) {
  return (
    <button className="primary-btn" {...props}>
      {title}
    </button>
  );
}
