import "./Button.css";

type Props = {
  title: string;
};

export default function Button({ title }: Props) {
  return (
    <button className="primary-btn">
      {title}
    </button>
  );
}